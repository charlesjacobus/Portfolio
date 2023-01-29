import { AfterViewInit, Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';

import { endsWith, isArray, isNil, startsWith } from 'lodash';

import { IExhibit, IExhibitSummary } from '../../models/exhibit';
import { IWork } from '../../models/work';
import { ExhibitService } from '../../services/exhibit.service';

@Component({
    template: 'No UI; this component is a base class for portfolio work, including exhibits and prose'
})
export abstract class WorkComponent implements AfterViewInit, OnInit {
    private summary: IExhibitSummary;

    public exhibit: IExhibit;

    constructor(protected exhibitService: ExhibitService, protected router: Router) { }

    @Input() public exhibitIndex: number; // A zero-based index for the exhibit

    @Input()
    set exhibitSummary(value: IExhibitSummary) {
        this.summary = value;
        this.exhibit = ExhibitService.createExhibit(value);
    }
    get exhibitSummary() {
        return this.summary;
    }

    public ngAfterViewInit(): void { }

    public ngOnInit(): void {
        this.initialize();
    }

    public getAssetsFolderName(): string {
        return 'images';
    }

    public getCurrentWorkOfLabel(): string {
        // If there's only 1 work in the exhibit, the label isn't that useful
        if (this.exhibit.works.length === 1) {
            return null;
        }

        return isNil(this.exhibit) ? null : `1 of ${this.exhibit.works.length.toLocaleString()}`;
    }

    public getExhibitAnchor(): string {
        return isNil(this.exhibit) ? null : this.exhibit.anchor;
    }

    public getExhibitDescription(): string {
        return isNil(this.exhibit) ? null : this.exhibit.description;
    }

    public getExhibitDescriptionUrl(): string {
        return isNil(this.exhibit) || isNil(this.exhibit.descriptionFileName) ? null : this.getFileFullName(this.exhibit.descriptionFileName);
    }

    public getExhibitName(): string {
        return isNil(this.exhibit) ? null : this.exhibit.name;
    }

    public getExhibitTextLabel(): string {
        return isNil(this.exhibit) || isNil(this.exhibit.textLabel) ? 'Text' : this.exhibit.textLabel;
    }

    public getExhibitTextRoute(): string {
        return isNil(this.exhibit) || isNil(this.exhibit.textRoute) ? null : this.exhibit.textRoute;
    }

    public getExhibitWorks(): Array<IWork> {
        return isNil(this.exhibit) ? [] : this.exhibit.works;
    }

    public isGalleryExhibit(): boolean {
        return !isNil(this.exhibit) && !endsWith(this.exhibit.promo.fileName, '.md');
    }

    public visitExhibitTextRoute(): void {
        if (isNil(this.exhibit) || isNil(this.exhibit.textRoute)) {
            return;
        }

        let textRoute: string = this.exhibit.textRoute;
        if (!startsWith(textRoute, '/')) {
            textRoute = '/'.concat(textRoute);
        }

        this.router.navigateByUrl(textRoute);
    }

    protected getExhibitWorkPropertyValue(index: number, propertyName: string): string {
        let works: Array<IWork> = this.getExhibitWorks();
        if (!isArray(works)) {
            return null;
        }

        let work = works[index];
        if (isNil(work)) {
            return null;
        }

        return work[propertyName];
    }

    protected getFileFullName(fileName: string): string {
        if (isNil(fileName)) {
            return null;
        }

        return `assets/${this.getAssetsFolderName()}/${this.getExhibitAnchor()}/${fileName}`;
    }

    protected abstract initialize(): void;
}
