import { HttpClient } from '@angular/common/http';
import { Component, ViewEncapsulation } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';

@Component({
  selector: 'app-home',
  styleUrls: ['./home.component.css'],
  templateUrl: './home.component.html',
  encapsulation: ViewEncapsulation.None
})
export class HomeComponent {
    exhibitHtml: string;

    constructor(private http: HttpClient, private metaService: Meta, private titleService: Title) { }

    ngOnInit(): void {
        this.initializeMeta();
        this.initializeTitle();
        const htmlFilePath = '/assets/exhibits/exhibit.html';

        this.http.get(htmlFilePath, { responseType: 'text' }).subscribe(
            (data) => {
                this.exhibitHtml = data;
            }
        );
    }

    private initializeMeta(): void {
        const metaDescription = 'Charles Jacobus personal web site, an online gallery with a rotating selection of recent and past work';
        this.metaService.updateTag({ name: 'description', content: metaDescription });
        this.metaService.updateTag({ property: 'og:description', content: metaDescription });
    }

    private initializeTitle(): void {
        const currentTitle = this.titleService.getTitle();
        const baseName = currentTitle.split(':')[0].trim();
        this.titleService.setTitle(baseName);
    }
}
