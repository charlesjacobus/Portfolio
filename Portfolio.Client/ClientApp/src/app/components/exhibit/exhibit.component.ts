import { AfterViewInit, ChangeDetectorRef, Component, HostListener, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { find, isArray, isEmpty, isNil, split } from 'lodash';

import { IExhibit, IExhibitSummary } from '../../models/exhibit';
import { IWork } from '../../models/work';
import { ExhibitService } from '../../services/exhibit.service';
import { WorkComponent } from './work.component';

interface LightboxImage {
    src: string;
    description: string;
    label: string;
}

@Component({
    selector: 'exhibit',
    standalone: false,
    templateUrl: './exhibit.component.html',
    styleUrls: ['./exhibit.component.css']
})
export class ExhibitComponent extends WorkComponent implements AfterViewInit, OnInit {
    private static readonly DefaultPrefetchImageIndex: number = -1;

    public lightboxImages: LightboxImage[] = [];
    public lightboxOpen: boolean = false;
    public lightboxIndex: number = 0;
    public lightboxZoom: number = 1;
    public lightboxImageLoaded: boolean = false;
    public arrowsVisible: boolean = true;
    public prefetchImageSource!: string;
    public visibleTab: string = 'work';

    private autoDisplayExhibitName: string | null = null;
    private prefetchImageIndex: number = ExhibitComponent.DefaultPrefetchImageIndex;
    private touchStartX: number = 0;
    private touchStartY: number = 0;
    private arrowsTimeout: any;

    constructor(private route: ActivatedRoute, protected override exhibitService: ExhibitService, protected override router: Router, private cdr: ChangeDetectorRef) {
        super(exhibitService, router);
    }

    public override ngAfterViewInit(): void {
        super.ngAfterViewInit();

        this.initializeTabOptions();
    }

    public openLightbox(index: number): void {
        this.lightboxIndex = index;
        this.lightboxZoom = 1;
        this.lightboxImageLoaded = false;
        this.lightboxOpen = true;
        this.prefetchImageIndex = ExhibitComponent.DefaultPrefetchImageIndex;
        this.resetArrowsAutoHide();
        this.emitPreviewChange();
        document.body.style.overflow = 'hidden';
    }

    public closeLightbox(): void {
        this.lightboxOpen = false;
        this.lightboxZoom = 1;
        document.body.style.overflow = '';
        this.handleImagePreviewClose();
    }

    public onLightboxImageLoad(): void {
        this.lightboxImageLoaded = true;
    }

    public lightboxNext(): void {
        if (this.lightboxImages.length === 0) return;
        this.lightboxIndex = (this.lightboxIndex + 1) % this.lightboxImages.length;
        this.onImageChange();
    }

    public lightboxPrev(): void {
        if (this.lightboxImages.length === 0) return;
        this.lightboxIndex = (this.lightboxIndex - 1 + this.lightboxImages.length) % this.lightboxImages.length;
        this.onImageChange();
    }

    public lightboxZoomIn(): void {
        this.lightboxZoom = Math.min(this.lightboxZoom + 0.25, 5);
    }

    public lightboxZoomOut(): void {
        this.lightboxZoom = Math.max(this.lightboxZoom - 0.25, 0.5);
    }

    public lightboxResetZoom(): void {
        this.lightboxZoom = 1;
    }

    public toggleFullscreen(): void {
        if (!document.fullscreenElement) {
            document.documentElement.requestFullscreen?.();
        } else {
            document.exitFullscreen?.();
        }
    }

    public onBackdropClick(event: MouseEvent): void {
        if ((event.target as HTMLElement).classList.contains('lightbox-overlay')) {
            this.closeLightbox();
        }
    }

    public onLightboxWheel(event: WheelEvent): void {
        event.preventDefault();
        if (event.deltaY < 0) {
            this.lightboxZoomIn();
        } else {
            this.lightboxZoomOut();
        }
    }

    public onTouchStart(event: TouchEvent): void {
        if (event.touches.length === 1) {
            this.touchStartX = event.touches[0].clientX;
            this.touchStartY = event.touches[0].clientY;
        }
    }

    public onTouchEnd(event: TouchEvent): void {
        // Only swipe-navigate when not zoomed in
        if (event.changedTouches.length === 1 && this.lightboxZoom <= 1) {
            const deltaX = event.changedTouches[0].clientX - this.touchStartX;
            const deltaY = event.changedTouches[0].clientY - this.touchStartY;
            if (Math.abs(deltaX) > 50 && Math.abs(deltaX) > Math.abs(deltaY)) {
                if (deltaX < 0) {
                    this.lightboxNext();
                } else {
                    this.lightboxPrev();
                }
            }
        }
    }

    public onLightboxMouseMove(): void {
        this.arrowsVisible = true;
        this.resetArrowsAutoHide();
    }

    public getCurrentLightboxImage(): LightboxImage | null {
        return this.lightboxImages[this.lightboxIndex] || null;
    }

    public getPromoImageSrc(): string | null {
        if (isNil(this.exhibitSummary) || isNil(this.exhibitSummary.promo)) {
            return null;
        }
        return this.getFileFullName(this.exhibitSummary.promo.fileName);
    }

    @HostListener('document:keydown', ['$event'])
    public onKeyDown(event: KeyboardEvent): void {
        if (!this.lightboxOpen) return;
        switch (event.key) {
            case 'Escape':
                this.closeLightbox();
                break;
            case 'ArrowRight':
                this.lightboxNext();
                break;
            case 'ArrowLeft':
                this.lightboxPrev();
                break;
        }
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

        this.exhibitService.fetchExhibit(this.exhibitSummary.id)
            .subscribe((result: IExhibit) => {
                this.exhibit = result;

                this.initializeLightboxImages(this.exhibit);

                // If the exhibits route is for a specific exhibit, then trigger scrolling and conditionally auto-display
                // Auto-display only exhibits that are not text-focused; otherwise, scroll into view only
                this.route.params.subscribe((params) => {
                    if (!isNil(this.exhibitSummary) && this.isGalleryExhibit()) {
                        const exhibitSummary = find <IExhibitSummary>(this.exhibitService.exhibitSummaries, ['anchor', params['exhibitIdentifier'] || '']);
                        if (!isNil(exhibitSummary) && exhibitSummary.id === this.exhibitSummary.id) {
                            if (this.exhibit.textIsDefault) {
                                this.scrollExhibitIntoView(exhibitSummary.anchor);
                            } else {
                                this.autoDisplayExhibitName = exhibitSummary.anchor; // Deferred, until the lightbox images are ready
                            }
                        }
                    }
                });

                this.route.fragment.subscribe((fragment) => {
                    if (!isNil(this.exhibitSummary) && this.isGalleryExhibit() && isNil(this.autoDisplayExhibitName)) {
                        const exhibitSummary = find<IExhibitSummary>(this.exhibitService.exhibitSummaries, ['anchor', fragment || '']);
                        if (!isNil(exhibitSummary) && exhibitSummary.id === this.exhibitSummary.id) {
                            this.scrollExhibitIntoView(exhibitSummary.anchor);
                        }
                    }
                });

                // Auto-display if route matched a non-text exhibit (replaces ngx-gallery imagesReady event)
                this.triggerAutoDisplay();
            });
    }

    private onImageChange(): void {
        this.lightboxZoom = 1;
        this.lightboxImageLoaded = false;
        this.emitPreviewChange();
        this.resetArrowsAutoHide();
    }

    private triggerAutoDisplay(): void {
        if (isNil(this.autoDisplayExhibitName)) {
            return;
        }

        // If there's an exhibit on the route, scroll it into view and auto-display it
        const that = this;
        setTimeout(function () {
            that.scrollExhibitIntoView(that.autoDisplayExhibitName);
            that.openLightbox(0);
        }, 250);
    }

    private handleImagePreviewClose(): void {
        this.prefetchImageIndex = ExhibitComponent.DefaultPrefetchImageIndex;

        // The current route behavior is
        //    - If an exhibit route matches, then scroll to it and auto-display the exhibit
        //    - On closing, revert to the general exhibits route
        //        - Subsequent user-activity of displaying exhibits does not change the route (remains on exhibits)
        this.router.navigate(['exhibits'], { replaceUrl: true });
        this.autoDisplayExhibitName = null;
    }

    private emitPreviewChange(): void {
        // Improves the image gallery experience by pre-fetching images
        const currentImage = this.getCurrentLightboxImage();
        if (isNil(currentImage) || isNil(currentImage.src)) {
            return;
        }

        const imagePathParts = split(currentImage.src, '/');
        if (imagePathParts.length < 3) {
            return;
        }

        this.exhibitService.fetchExhibitSummaries().subscribe((exhibitSummaries: Array<IExhibitSummary>) => {
            const exhibitSummary = find<IExhibitSummary>(exhibitSummaries, ['anchor', imagePathParts[2]]);
            if (!isNil(exhibitSummary)) {
                this.exhibitService.fetchExhibit(exhibitSummary.id).subscribe((exhibit: IExhibit) => {
                    if (!isNil(exhibit) && isArray(exhibit.works)) {
                        this.prefetchImageIndex =
                            (
                                this.prefetchImageIndex === ExhibitComponent.DefaultPrefetchImageIndex ||   // Preview image displayed
                                this.lightboxIndex === this.prefetchImageIndex + 1 ||                       // Next image
                                this.lightboxIndex === 0 && this.prefetchImageIndex === exhibit.works.length - 1   // Last-to-first image
                            ) ?
                            this.lightboxIndex + 1 <= exhibit.works.length - 1 ? this.lightboxIndex + 1 : 0 :      // Forward
                            this.lightboxIndex - 1 >= 0 ? this.lightboxIndex - 1 : exhibit.works.length - 1;       // Backward

                        const work: IWork = exhibit.works[this.prefetchImageIndex];
                        if (!isNil(work)) {
                            this.prefetchImageSource = this.getFileFullName(work.fileNameLarge);
                        }

                        this.prefetchImageIndex = this.lightboxIndex;
                    }
                });
            }
        });
    }

    private initializeLightboxImages(exhibit: IExhibit): void {
        if (isNil(exhibit) || isNil(exhibit.works) || !this.isGalleryExhibit()) {
            return;
        }

        this.lightboxImages = [];
        exhibit.works.forEach((work: IWork) => {
            this.lightboxImages.push({
                src: this.getFileFullName(work.fileNameLarge),
                description: !isEmpty(work.name) && !isEmpty(work.description) ? `${work.name} : ${work.description}` : !isEmpty(work.description) ? work.description : work.name,
                label: !isEmpty(work.name) ? work.name : work.fileName
            });
        });
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

    private resetArrowsAutoHide(): void {
        if (this.arrowsTimeout) {
            clearTimeout(this.arrowsTimeout);
        }
        this.arrowsVisible = true;
        this.arrowsTimeout = setTimeout(() => {
            this.arrowsVisible = false;
        }, 3000);
    }

    private scrollExhibitIntoView(exhibitAnchor: string | null): void {
        if (isNil(exhibitAnchor)) {
            return;
        }

        const elements: NodeListOf<HTMLElement> = document.getElementsByName(exhibitAnchor);
        if (!isNil(elements) && elements.length === 1) {
            elements[0].scrollIntoView();
        }
    }
}
