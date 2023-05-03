import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { find, isArray, isNil } from 'lodash';

import { IExhibit, IExhibitSummary } from '../models/exhibit';
import { ILeet } from '../models/leet';
import { AppConfigService } from './config.service';
import { DataService } from './data.service';

export interface IExhibitService {
    fetchExhibit(id: number): Observable<IExhibit>;
    fetchExhibitSummaries(): Observable<Array<IExhibitSummary>>;
    fetchLeet(code: string): Observable<ILeet>;
}

@Injectable()
export class ExhibitService implements IExhibitService {
    private exhibits: Array<IExhibit> = [];

    public exhibitSummaries: Array<IExhibitSummary>;

    constructor(private dataService: DataService) { }

    public fetchExhibitSummaries(): Observable<Array<IExhibitSummary>> {
        return new Observable(observer => {
            if (isArray(this.exhibitSummaries)) {
                observer.next(this.exhibitSummaries);
                observer.complete();
            } else {
                let url: string = AppConfigService.portfolioInfo.hrefGetActiveExhibits;

                this.dataService.get(url)
                    .subscribe((result: Array<IExhibitSummary>) => {
                        this.exhibitSummaries = result;

                        observer.next(result);
                        observer.complete();
                    });
            }
        });
    }

    public fetchExhibit(id: number): Observable<IExhibit> {
        return new Observable(observer => {
            let exhibit: IExhibit = find(this.exhibits, ['id', id]);
            if (!isNil(exhibit)) {
                observer.next(exhibit);
                observer.complete();
            } else {
                this.exhibits = [];
                this.dataService.get(AppConfigService.portfolioInfo.hrefGetExhibit.replace('{id}', id.toString()))
                    .subscribe((result: IExhibit) => {
                        this.exhibits.push(result);

                        observer.next(result);
                        observer.complete();
                    });
            }
        });
    }

    public fetchLeet(code: string): Observable<ILeet> {
        let url: string = AppConfigService.portfolioInfo.hrefGetLeet;
        if (!isNil(code)) {
            url = url.concat('?code='.concat(code));
        }

        return new Observable(observer => {
            return this.dataService.get<ILeet>(url)
                .subscribe((response: ILeet) => {
                    observer.next(response);
                    observer.complete();
                });
        });
    }

    public static createExhibit(exhibitSummary: IExhibitSummary): IExhibit {
        if (isNil(exhibitSummary)) {
            return null;
        }

        let exhibit: IExhibit = {
            id: exhibitSummary.id,
            name: exhibitSummary.name,
            description: exhibitSummary.description,
            descriptionFileName: exhibitSummary.descriptionFileName,
            anchor: exhibitSummary.anchor,
            promo: exhibitSummary.promo,
            works: [exhibitSummary.promo],
            textIsDefault: exhibitSummary.textIsDefault,
            textLabel: exhibitSummary.textLabel,
            textRoute: exhibitSummary.textRoute
        };

        return exhibit;
    }
}
