import { AfterViewInit, Component, OnInit } from '@angular/core';

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

    constructor(private exhibitService: ExhibitService) { }

    public ngAfterViewInit(): void { }

    public ngOnInit(): void {
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
}
