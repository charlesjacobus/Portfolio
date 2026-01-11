import { Component } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

import { Observable } from 'rxjs';

import { isNil, toString } from 'lodash';

import { IPhoto } from '../../models/photo';
import { AppConfigService } from '../../services/config.service';
import { DataService } from '../../services/data.service';

@Component({
  selector: 'about',
    templateUrl: './about.component.html',
    styleUrls: ['./about.component.css']
})
export class AboutComponent {
    private photo: IPhoto;

    constructor(private dataService: DataService, private metaService: Meta, private titleService: Title) {
        this.initialize();
    }

    public getPhotoCaptionHtml(): string {
        return isNil(this.photo) ? '' : this.photo.captionHtml;
    }

    public getPhotoImageSource(): string {
        return isNil(this.photo) ? '' : this.photo.source;
    }

    public getPhotoImageTitle(): string {
        return isNil(this.photo) ? '' : this.photo.title;
    }

    public getPhotoImageWidth(): number {
        return !isNil(this.photo) && this.photo.orientation === 'horizontal' ? 500 : 300;
    }

    public photoIsAvailable(): boolean {
        return !isNil(this.photo) && !isNil(this.photo.source);
    }

    private fetchPhoto(): Observable<IPhoto> {
        return new Observable(observer => {
            const url: string = AppConfigService.portfolioInfo.hrefGetPhoto.replace('{id}', toString(0)); // 0 ensures a random photo, for fun
            this.dataService.get(url)
                .subscribe((result: IPhoto) => {
                    this.photo = result;

                    observer.next(result);
                    observer.complete();
                });
        });
    }

    private initialize(): void {
        this.initializeTitle();
        this.initializeMeta();
        this.fetchPhoto().subscribe();
    }

    private initializeMeta(): void {
        const metaDescription = 'Learn about the artist Charles Jacobus, including his background, artists statement, and purpose of this site';
        this.metaService.updateTag({ name: 'description', content: metaDescription });
        this.metaService.updateTag({ property: 'og:description', content: metaDescription });
    }

    private initializeTitle(): void {
        const currentTitle = this.titleService.getTitle();
        const baseName = currentTitle.split(':')[0].trim();
        this.titleService.setTitle(`${baseName} : About`);
    }
}
