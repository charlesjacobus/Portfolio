import { AfterViewInit, Component, Input, OnInit } from '@angular/core';

import * as moment from 'moment';

@Component({
    selector: 'copyright',
    templateUrl: './copyright.component.html',
    styleUrls: ['./copyright.component.css']
})
export class CopyrightComponent implements AfterViewInit, OnInit {
    constructor() { }

    @Input()
    public aboutLinkActive: boolean = false;

    ngAfterViewInit(): void { }

    ngOnInit(): void { }

    public getCurrentYear(): string {
        return moment().format('YYYY');
    }
}
