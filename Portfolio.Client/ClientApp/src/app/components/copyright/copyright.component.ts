import { Component, Input } from '@angular/core';

import { isNil } from 'lodash';

import { AppConfigService } from '../../services/config.service';

@Component({
    selector: 'copyright',
    templateUrl: './copyright.component.html',
    styleUrls: ['./copyright.component.css']
})
export class CopyrightComponent {
    public static readonly DefaultPortfolioInformationalVersion: string = '1.0.0';
    public static readonly DefaultPortfolioName: string = 'Portfolio';

    private currentYearValue: string;

    constructor() {
        this.currentYearValue = new Date().getFullYear().toString();
    }

    @Input()
    public aboutLinkActive: boolean = false;

    public getApplicationName(): string {
        return !isNil(AppConfigService.portfolioInfo) && !isNil(AppConfigService.portfolioInfo.settings) ?
            AppConfigService.portfolioInfo.settings.applicationName || CopyrightComponent.DefaultPortfolioName :
            CopyrightComponent.DefaultPortfolioName;
    }

    public getCurrentYear(): string {
        return this.currentYearValue;
    }

    public getInformationalVersion(): string {
        return !isNil(AppConfigService.portfolioInfo) && !isNil(AppConfigService.portfolioInfo.settings) ?
            AppConfigService.portfolioInfo.settings.informationalVersion || CopyrightComponent.DefaultPortfolioInformationalVersion :
            CopyrightComponent.DefaultPortfolioInformationalVersion;
    }
}
