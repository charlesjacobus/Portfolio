import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

export interface IDataService {
    get<T>(url: string);
    post<T>(url: string, object: any);
}

@Injectable()
export class DataService implements IDataService {
    public static httpOptions = {
        headers: new HttpHeaders({
            'Accept': 'application/json', 'Content-Type': 'application/json'
        }), withCredentials: false
    };

    constructor(private http: HttpClient) { }

    public get<T>(url: string): Observable<T> {
        return this.http.get<T>(url, DataService.httpOptions)
            .pipe(
                catchError((err: any) => {
                    return throwError(err);
                })
            );
    }

    public post<T>(url: string, object: any): Observable<T> {
        return this.http.post<T>(url, object, DataService.httpOptions)
            .pipe(
                catchError((err: any) => {
                    return throwError(err);
                })
            );
    }
}
