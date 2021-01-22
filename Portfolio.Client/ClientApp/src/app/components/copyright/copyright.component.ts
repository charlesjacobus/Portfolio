import { AfterViewInit, Component, Input, OnInit } from '@angular/core';

import { isNil } from 'lodash';
import * as moment from 'moment';

import { AppConfigService } from '../../services/config.service';

@Component({
    selector: 'copyright',
    templateUrl: './copyright.component.html',
    styleUrls: ['./copyright.component.css']
})
export class CopyrightComponent implements AfterViewInit, OnInit {
    constructor() { }

    @Input()
    public aboutLinkActive: boolean = false;

    public ngAfterViewInit(): void { }

    public ngOnInit(): void { }

    public getApplicationName(): string {
        if (!isNil(AppConfigService.portfolioInfo) && !isNil(AppConfigService.portfolioInfo.settings)) {
            return AppConfigService.portfolioInfo.settings.applicationName;
        }

        return 'Portfolio';
    }

    public getCurrentYear(): string {
        return moment().format('YYYY');
    }

    public getInformationalVersion(): string {
        if (!isNil(AppConfigService.portfolioInfo) && !isNil(AppConfigService.portfolioInfo.settings)) {
            return AppConfigService.portfolioInfo.settings.informationalVersion;
        }

        return '1.0.0';
    }
}
