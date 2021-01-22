import { Component } from '@angular/core';

import { isNil } from 'lodash';

import { AppConfigService } from '../../services/config.service';

@Component({
    selector: 'app-nav-menu',
    templateUrl: './nav-menu.component.html',
    styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
    public isExpanded = false;

    public collapse(): void {
        this.isExpanded = false;
    }

    public getApplicationName(): string {
        if (!isNil(AppConfigService.portfolioInfo) && !isNil(AppConfigService.portfolioInfo.settings)) {
            return AppConfigService.portfolioInfo.settings.applicationName;
        }

        return 'Portfolio';
    }

    public toggle(): void {
        this.isExpanded = !this.isExpanded;
    }
}
