import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { find, isArray, isEmpty, isNil, split } from 'lodash';

import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryComponent } from 'ngx-gallery-9';

import { IExhibit, IExhibitSummary } from '../../models/exhibit';
import { IWork } from '../../models/work';
import { ExhibitService } from '../../services/exhibit.service';
import { WorkComponent } from './work.component';

@Component({
    selector: 'exhibit',
    templateUrl: './exhibit.component.html',
    styleUrls: ['./exhibit.component.css']
})
export class ExhibitComponent extends WorkComponent implements AfterViewInit, OnInit {
    private static readonly DefaultPrefetchImageIndex: number = -1;

    public galleryOptions: NgxGalleryOptions[];
    public galleryImages: NgxGalleryImage[];
    public prefetchImageSource: string;
    public visibleTab: string = 'work';

    private autoDisplayExhibitName: string = null;
    private prefetchImageIndex: number = ExhibitComponent.DefaultPrefetchImageIndex;

    @ViewChild('NgxGalleryComponent') private galleryComponent: NgxGalleryComponent;

    constructor(private route: ActivatedRoute, protected exhibitService: ExhibitService, protected router: Router) {
        super(exhibitService, router);
    }

    public ngAfterViewInit(): void {
        super.ngAfterViewInit();

        this.initializeTabOptions();
    }

    public handleImagePreviewChange(event: any): void {
        // Improves the image gallery experience by pre-fetching images
        // event.index is the 0-base index of the image just displayed
        // event.image.big is the relative URL of the image just displayed, without the host name or scheme

        if (isNil(event) || isNil(event.image) || isNil(event.image.big)) {
            return;
        }

        const imagePathParts = split(event.image.big, '/');
        if (imagePathParts.length < 3) {
            return;
        }

        this.exhibitService.fetchExhibitSummaries().subscribe((exhibitSummaries: Array<IExhibitSummary>) => {
            const exhibitSummary: IExhibitSummary = find(exhibitSummaries, ['anchor', imagePathParts[2]]);
            if (!isNil(exhibitSummary)) {
                this.exhibitService.fetchExhibit(exhibitSummary.id).subscribe((exhibit: IExhibit) => {
                    if (!isNil(exhibit) && isArray(exhibit.works)) {
                        this.prefetchImageIndex =
                            (
                                this.prefetchImageIndex === ExhibitComponent.DefaultPrefetchImageIndex ||   // Preview image displayed
                                event.index === this.prefetchImageIndex + 1 ||                              // Next image
                                event.index === 0 && this.prefetchImageIndex === exhibit.works.length - 1   // Last-to-first image
                            ) ?
                            event.index + 1 <= exhibit.works.length - 1 ? event.index + 1 : 0 :             // Forward
                            event.index - 1 >= 0 ? event.index - 1 : exhibit.works.length - 1;              // Backward

                        const work: IWork = exhibit.works[this.prefetchImageIndex];
                        if (!isNil(work)) {
                            this.prefetchImageSource = this.getFileFullName(work.fileNameLarge);
                        }

                        this.prefetchImageIndex = event.index;
                    }
                });
            }
        });
    }

    public handleImagePreviewClose(): void {
        this.prefetchImageIndex = ExhibitComponent.DefaultPrefetchImageIndex;

        // The current route behavior is
        //    - If an exhibit route matches, then scroll to it and auto-display the exhibit
        //    - Open closing, revert to the general exhibits route
        //        - Subsequent user-activity of displaying exhibits does not change the route (remains on exhibits) 
        this.router.navigate(['exhibits'], { replaceUrl: true });
        this.autoDisplayExhibitName = null;
    }

    public handleImagePreviewOpen(): void { }

    public handleImagesReady(): void {
        if (isNil(this.autoDisplayExhibitName) || isNil(this.galleryComponent)) {
            return;
        }

        // If there's an exhibit on the route, scroll it into view and auto-display it
        // The closure is required, since without it the gallery component preview displays without actually showing the images
        const that = this;
        setTimeout(function () {
            that.scrollExhibitIntoView(that.autoDisplayExhibitName);

            that.galleryComponent.openPreview(0);
        }, 250);
    }

    public showWorkText(): boolean {
        return (!!this.getExhibitDescription() || !!this.getExhibitDescriptionUrl());
    }

    public showNoWorkText(): boolean {
        return !this.getExhibitDescription() && !this.getExhibitDescriptionUrl();
    }

    protected initialize(): void {
        if (!this.isGalleryExhibit()) {
            return;
        }

        this.initializeGalleryOptions();

        this.exhibitService.fetchExhibit(this.exhibitSummary.id)
            .subscribe((result: IExhibit) => {
                this.exhibit = result;

                this.initializeGalleryImages(this.exhibit);

                // If the exhibits route is for a specific exhibit, then trigger scrolling and conditionally auto-display
                // Auto-display only exhibits that are not text-focused; otherwise, scroll into view only
                this.route.params.subscribe((params) => {
                    if (!isNil(this.exhibitSummary) && this.isGalleryExhibit()) {
                        const exhibitSummary: IExhibitSummary = find(this.exhibitService.exhibitSummaries, ['anchor', params['exhibitIdentifier'] || '']);
                        if (!isNil(exhibitSummary) && exhibitSummary.id === this.exhibitSummary.id) {
                            if (this.exhibit.textIsDefault) {
                                this.scrollExhibitIntoView(exhibitSummary.anchor);
                            } else {
                                this.autoDisplayExhibitName = exhibitSummary.anchor; // Deferred, until the gallery images are ready
                            }
                        }
                    }
                });

                this.route.fragment.subscribe((fragment) => {
                    if (!isNil(this.exhibitSummary) && this.isGalleryExhibit() && isNil(this.autoDisplayExhibitName)) {
                        const exhibitSummary: IExhibitSummary = find(this.exhibitService.exhibitSummaries, ['anchor', fragment || '']);
                        if (!isNil(exhibitSummary) && exhibitSummary.id === this.exhibitSummary.id) {
                            this.scrollExhibitIntoView(exhibitSummary.anchor);
                        }
                    }
                });
            });
    }

    private initializeGalleryImages(exhibit: IExhibit): void {
        if (isNil(exhibit) || isNil(exhibit.works) || !this.isGalleryExhibit()) {
            return;
        }

        this.galleryImages = [];
        exhibit.works.forEach((work: IWork) => {
            this.galleryImages.push({
                // Given a design where all the exhibit works are available only in big/preview mode, for the main/medium image we only need to load the promo image
                // This avoids unnecessarily loading main/medium images that will never be seen
                medium: this.getFileFullName(this.exhibitSummary.promo.fileName),
                big: this.getFileFullName(work.fileNameLarge),
                description: !isEmpty(work.name) && !isEmpty(work.description) ? `${work.name} : ${work.description}` : !isEmpty(work.description) ? work.description : work.name,
                label: !isEmpty(work.name) ? work.name : work.fileName
            });
        });
    }

    private initializeGalleryOptions(): void {
        this.galleryOptions = [
            {
                width: '100%',
                arrowNextIcon: 'fa fa-chevron-right',
                arrowPrevIcon: 'fa fa-chevron-left',
                imageArrows: false,
                lazyLoading: false,
                previewArrowsAutoHide: true,
                previewCloseOnClick: true,
                previewCloseOnEsc: true,
                previewDescription: true,
                previewFullscreen: true,
                previewKeyboardNavigation: true,
                previewInfinityMove: true,
                previewSwipe: true,
                previewZoom: true,
                thumbnails: false
            },
            {
                breakpoint: 800,
                width: '100%',
                height: '250px',  
                imagePercent: 80
            },
            {
                breakpoint: 400,
                width: '100%',
                height: '125px',
                imagePercent: 80
            }
        ];
    }

    private initializeTabOptions(): void {
        // Cheating a little bit here, because this probably wouldn't be the best UX for exhibits above the fold
        // But this simply allows us to pre-load the exhibit promo image and default to the "text" tab, without Angular complaining about it
        const that = this;
        setTimeout(function () {
            if (that.exhibit.textIsDefault) {
                that.visibleTab = 'text';
            }
        }, 1000);
    }

    private scrollExhibitIntoView(exhibitAnchor: string): void {
        if (isNil(exhibitAnchor)) {
            return;
        }

        const elements: NodeListOf<HTMLElement> = document.getElementsByName(exhibitAnchor);
        if (!isNil(elements) && elements.length === 1) {
            elements[0].scrollIntoView();
        }
    }
}
