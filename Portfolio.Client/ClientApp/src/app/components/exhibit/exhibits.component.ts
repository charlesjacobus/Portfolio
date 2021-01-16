import { AfterViewInit, Component, OnInit } from '@angular/core';

import { IExhibitSummary } from '../../models/exhibit';
import { ExhibitService } from '../../services/exhibit.service';

@Component({
    selector: 'exhibits',
    templateUrl: './exhibits.component.html',
    styleUrls: ['./exhibits.component.css']
})
export class ExhibitsComponent implements AfterViewInit, OnInit {
    constructor(private exhibitService: ExhibitService) { }

    public ngAfterViewInit(): void { }

    public ngOnInit(): void {
        this.exhibitService.fetchExhibitSummaries()
            .subscribe((result: Array<IExhibitSummary>) => { });
    }

    public getExhibitSummaries(): Array<IExhibitSummary> {
        return this.exhibitService.exhibitSummaries || [];
    }
}
