import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpClient } from '@angular/common/http';

import { throwError } from 'rxjs';

import { IConfig } from '../models/config';
import { IPortfolioInfo } from '../models/portfolioInfo';
import { DataService } from './data.service';

import { isNil, replace } from 'lodash';
import * as urljoin from 'url-join';

@Injectable()
export class AppConfigService {
    private static configuration: IConfig;

    public static api = '';
    public static apiInfo = '';
    public static portfolioInfo: IPortfolioInfo;

    constructor(private http: HttpClient, private dataService: DataService) { }

    public load() {
        const rootConfigurationFile = `assets/config.json`;

        return this.http.get<IConfig>(rootConfigurationFile)
            .toPromise()
            .then((response) => {
                AppConfigService.configuration = response;

                if (!this.useExplicitPerEnvironmentConfiguration(response)) {
                    AppConfigService.api = this.getApiHostName();
                    AppConfigService.apiInfo = urljoin(AppConfigService.api, response.apiInfoPath);
                }

                return AppConfigService.configuration;
            })
            .then((config: IConfig) => {
                return AppConfigService.apiInfo;
            })
            .then((url: string) => {
                return this.dataService.get<IPortfolioInfo>(url)
                    .toPromise()
                    .then((info) => {
                        AppConfigService.portfolioInfo = info;
                    });
            })
            .catch(this.handleError);
    }

    private getApiHostName(): string {
        let hostName = document.location.origin;

        if (hostName.indexOf('localhost') !== -1) {
            // Scully app and static ports
            // hostName = replace(hostName, '1668', AppConfigService.configuration.localhostWebPort);
            // hostName = replace(hostName, '1864', AppConfigService.configuration.localhostWebPort);

            hostName = replace(hostName, AppConfigService.configuration.localhostWebPort.toString(),
                AppConfigService.configuration.localhostApiPort.toString());

            if (AppConfigService.configuration.apiUseSsl) {
                hostName = replace(hostName, 'http://', 'https://');
            }
        }
        else {
            hostName = replace(hostName, AppConfigService.configuration.hostWeb, AppConfigService.configuration.hostApi);
            hostName = replace(hostName, 'www.', '');
        }

        return hostName;
    }

    private handleError(error: HttpErrorResponse) {
        if (error.error instanceof ErrorEvent) {
            console.error('An error occurred:', error.error.message);
        } else {
            console.error(
                `Backend returned code ${error.status}, ` +
                `body was: ${error.error}`);
        }
        return throwError(
            'Could not load config file; please try again later.');
    }

    private useExplicitPerEnvironmentConfiguration(config: IConfig): boolean {
        return (!isNil(config) && !isNil(config.explicitPerEnvironmentConfiguration) && !!config.explicitPerEnvironmentConfiguration);
    }
}
