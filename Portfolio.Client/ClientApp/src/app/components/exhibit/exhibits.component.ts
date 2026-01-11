import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

import { IExhibitSummary } from '../../models/exhibit';
import { ExhibitService } from '../../services/exhibit.service';

@Component({
    selector: 'exhibits',
    templateUrl: './exhibits.component.html',
    styleUrls: ['./exhibits.component.css']
})
export class ExhibitsComponent implements AfterViewInit, OnInit {
    public errorFetchingExhibitSummaries: boolean = false;
    public exhibitsLoaded: boolean = false;

    constructor(private exhibitService: ExhibitService, private metaService: Meta, private titleService: Title) { }

    public ngAfterViewInit(): void { }

    public ngOnInit(): void {
        this.initializeMeta();
        this.initializeTitle();
        this.exhibitService.fetchExhibitSummaries()
            .subscribe(
                {
                    next: (result: Array<IExhibitSummary>) => {
                        this.exhibitsLoaded = true;
                    },
                    error: () => { this.errorFetchingExhibitSummaries = true; },
                    complete: () => {}
                });
    }

    public getExhibitSummaries(): Array<IExhibitSummary> {
        return this.exhibitService.exhibitSummaries || [];
    }

    private initializeMeta(): void {
        const metaDescription = 'View a rotating selection of paintings, drawings, and other work by Charles Jacobus - a virtual gallery';
        this.metaService.updateTag({ name: 'description', content: metaDescription });
        this.metaService.updateTag({ property: 'og:description', content: metaDescription });
    }

    private initializeTitle(): void {
        const currentTitle = this.titleService.getTitle();
        const baseName = currentTitle.split(':')[0].trim();
        this.titleService.setTitle(`${baseName} : Exhibits`);
    }
}
