import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';

import { isArray } from 'lodash';

import { IWriting } from '../models/writing';
import { AppConfigService } from './config.service';
import { DataService } from './data.service';

export interface IWritingService {
    fetchWritings(): Observable<Array<IWriting>>;
}

@Injectable()
export class WritingService implements IWritingService {
    public writings: Array<IWriting>;

    constructor(private dataService: DataService) { }

    public fetchWritings(): Observable<Array<IWriting>> {
        return new Observable(observer => {
            if (isArray(this.writings)) {
                observer.next(this.writings);
                observer.complete();
            } else {
                let url: string = AppConfigService.portfolioInfo.hrefGetActiveWritings;

                this.dataService.get(url)
                    .subscribe((result: Array<IWriting>) => {
                        this.writings = result;

                        observer.next(result);
                        observer.complete();
                    });
            }
        });
    }
}
