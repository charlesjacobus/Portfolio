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

    public exhibitSummaries: Array<IExhibitSummary> | null = null;

    constructor(private dataService: DataService) { }

    public fetchExhibitSummaries(): Observable<Array<IExhibitSummary>> {
        return new Observable(observer => {
            if (isArray(this.exhibitSummaries)) {
                observer.next(this.exhibitSummaries);
                observer.complete();
            } else {
                let url: string = AppConfigService.portfolioInfo.hrefGetActiveExhibits;

                return this.dataService.get<Array<IExhibitSummary>>(url)
                    .subscribe({
                        next: (result: Array<IExhibitSummary>) => {
                            this.exhibitSummaries = result;

                            observer.next(result);
                            observer.complete();
                        },
                        error: (err: any) => observer.error(err)
                    });
            }

            return;
        });
    }

    public fetchExhibit(id: number): Observable<IExhibit> {
        return new Observable(observer => {
            let exhibit = find<IExhibit>(this.exhibits, ['id', id]);
            if (!isNil(exhibit)) {
                observer.next(exhibit);
                observer.complete();
            } else {
                this.exhibits = [];
                return this.dataService.get<IExhibit>(AppConfigService.portfolioInfo.hrefGetExhibit.replace('{id}', id.toString()))
                    .subscribe({
                        next: (result: IExhibit) => {
                            this.exhibits.push(result);

                            observer.next(result);
                            observer.complete();
                        },
                        error: (err: any) => observer.error(err)
                    });
            }

            return;
        });
    }

    public fetchLeet(code: string | null): Observable<ILeet> {
        let url: string = AppConfigService.portfolioInfo.hrefGetLeet;
        if (!isNil(code)) {
            url = url.concat('?code='.concat(code));
        }

        return new Observable(observer => {
            return this.dataService.get<ILeet>(url)
                .subscribe({
                    next: (response: ILeet) => {
                        observer.next(response);
                        observer.complete();
                    },
                    error: (err: any) => observer.error(err)
                });
        });
    }

    public static createExhibit(exhibitSummary: IExhibitSummary): IExhibit | null {
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
            works: !isNil(exhibitSummary.promo) ? [exhibitSummary.promo] : [],
            textIsDefault: exhibitSummary.textIsDefault,
            textLabel: exhibitSummary.textLabel,
            textRoute: exhibitSummary.textRoute
        };

        return exhibit;
    }
}
