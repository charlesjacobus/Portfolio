import { Injectable } from '@angular/core';
import { HttpErrorResponse, HttpClient } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';

import { IConfig } from '../models/config';
import { IPortfolioInfo } from '../models/portfolioInfo';
import { DataService } from './data.service';

import { replace } from 'lodash';

export interface IAppConfigService {
    load(): Promise<void | Observable<never>>;
}

@Injectable()
export class AppConfigService implements IAppConfigService {
    private static configuration: IConfig;

    public static api = '';
    public static apiInfo = '';
    public static portfolioInfo: IPortfolioInfo;

    constructor(private http: HttpClient, private dataService: DataService) { }

    public load(): Promise<void | Observable<never>> {
        const rootConfigurationFile = `assets/config.json`;

        return this.http.get<IConfig>(rootConfigurationFile)
            .toPromise()
            .then((response) => {
                AppConfigService.configuration = response;

                AppConfigService.api = this.getApiHostName();
                AppConfigService.apiInfo = new URL(response.apiInfoPath, AppConfigService.api).href;

                return AppConfigService.configuration;
            })
            .then(() => {
                return this.dataService.get<IPortfolioInfo>(AppConfigService.apiInfo)
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
}
