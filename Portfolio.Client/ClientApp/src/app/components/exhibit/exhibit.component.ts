import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { find, isArray, isEmpty, isNil, split } from 'lodash';

import { NgxGalleryOptions, NgxGalleryImage } from 'ngx-gallery-9';

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

    private prefetchImageIndex: number = ExhibitComponent.DefaultPrefetchImageIndex;

    constructor(protected exhibitService: ExhibitService, protected router: Router) {
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
    }

    public showWorkText(): boolean {
        return (!!this.getExhibitDescription() || !!this.getExhibitDescriptionUrl());
    }

    public showNoWorkText(): boolean {
        return !this.getExhibitDescription() && !this.getExhibitDescriptionUrl();
    }

    protected getAssetsFolderName(): string {
        return 'images';
    }

    protected initialize(): void {
        if (!this.isGalleryExhibit()) {
            return;
        }

        this.exhibitService.fetchExhibit(this.exhibitSummary.id)
            .subscribe((result: IExhibit) => {
                this.exhibit = result;

                this.initializeGalleryImages(this.exhibit);
            });

        this.initializeGalleryOptions();
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
}
