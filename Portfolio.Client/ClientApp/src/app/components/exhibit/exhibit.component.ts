import { AfterViewInit, Component, OnInit } from '@angular/core';

import { isNil } from 'lodash';

import { NgxGalleryOptions, NgxGalleryImage } from 'ngx-gallery-9';

import { IExhibit } from '../../models/exhibit';
import { IWork } from '../../models/work';
import { ExhibitService } from '../../services/exhibit.service';
import { WorkComponent } from './work.component';

@Component({
    selector: 'exhibit',
    templateUrl: './exhibit.component.html',
    styleUrls: ['./exhibit.component.css']
})
export class ExhibitComponent extends WorkComponent implements AfterViewInit, OnInit {
    public galleryOptions: NgxGalleryOptions[];
    public galleryImages: NgxGalleryImage[];
    public visibleTab: string = 'work';

    constructor(protected exhibitService: ExhibitService) {
        super(exhibitService);
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
                description: !isNil(work.description) ? work.description : work.name
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
                previewArrowsAutoHide: true,
                previewCloseOnEsc: true,
                previewDescription: true,
                previewFullscreen: true,
                previewKeyboardNavigation: true,
                previewInfinityMove: true,
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
}