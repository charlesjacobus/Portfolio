import { HttpClient } from '@angular/common/http';
import { Component, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'app-home',
  styleUrls: ['./home.component.css'],
  templateUrl: './home.component.html',
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent {
    exhibitHtml: string;

    constructor(private http: HttpClient) { }

    ngOnInit(): void {
        const htmlFilePath = '/assets/exhibits/exhibit.html';

        this.http.get(htmlFilePath, { responseType: 'text' }).subscribe(
            (data) => {
                this.exhibitHtml = data;
            }
        );
    }
}
