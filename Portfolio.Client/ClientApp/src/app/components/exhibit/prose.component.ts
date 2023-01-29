import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { isNil } from 'lodash';

import { IExhibit } from '../../models/exhibit';
import { ExhibitService } from '../../services/exhibit.service';
import { WorkComponent } from './work.component';

@Component({
    selector: 'prose-gallery',
    templateUrl: './prose.component.html',
    styleUrls: ['./prose.component.css']
})
export class ProseComponent extends WorkComponent implements AfterViewInit, OnInit {
    private currentWorkIndex: number = 0;

    // Reference: https://marked.js.org/#/README.md#specifications
    // Reference: https://spec.commonmark.org/dingus/
    
    constructor(protected exhibitService: ExhibitService, protected router: Router)
    {
        super(exhibitService, router);
    }

    public getAssetsFolderName(): string {
        return 'texts';
    }

    public getCurrentWorkName(): string {
        return this.getExhibitWorkPropertyValue(this.currentWorkIndex, 'name');
    }

    public getCurrentWorkOfLabel(): string {
        if (isNil(this.exhibit) || isNil(this.exhibit.works)) {
            return null;
        }

        return this.exhibit.works.length === 1 ? null : `${(this.currentWorkIndex + 1).toLocaleString()} of ${this.exhibit.works.length.toLocaleString()}`;
    }

    public getCurrentWorkUrl(): string {
        let folder: string = this.getExhibitAnchor();
        if (isNil(folder)) {
            return null;
        }

        let fileName: string = this.getExhibitWorkPropertyValue(this.currentWorkIndex, 'fileName');

        return this.getFileFullName(fileName);
    }

    public incrementCurrentWorkIndex(): void {
        this.currentWorkIndex++;

        if (this.currentWorkIndex > this.exhibit.works.length - 1) {
            this.currentWorkIndex = 0;
        }
    }

    protected initialize(): void {
        if (this.isGalleryExhibit()) {
            return;
        }

        this.exhibitService.fetchExhibit(this.exhibitSummary.id)
            .subscribe((result: IExhibit) => {
                this.exhibit = result;
            });
    }
}
