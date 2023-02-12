import { BrowserModule } from '@angular/platform-browser';
import { HammerModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { ClipboardModule } from 'ngx-clipboard';
import { MarkdownModule, MarkedOptions } from 'ngx-markdown';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxGalleryModule } from 'ngx-gallery-9';

import { ScullyLibModule } from '@scullyio/ng-lib';

import { AboutComponent } from './components/about/about.component';
import { AppComponent } from './app.component';
import { CopyrightComponent } from './components/copyright/copyright.component';
import { ExhibitComponent } from './components/exhibit/exhibit.component';
import { ExhibitsComponent } from './components/exhibit/exhibits.component';
import { HomeComponent } from './components/home/home.component';
import { LeetsComponent } from './components/exhibit/leets.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { ProseComponent } from './components/exhibit/prose.component';

import { AppConfigService } from './services/config.service';
import { DataService } from './services/data.service';
import { ExhibitService } from './services/exhibit.service';

export function initConfig(configService: AppConfigService) {
    return () => configService.load();
}

@NgModule({
    declarations: [
        AboutComponent,
        AppComponent,
        CopyrightComponent,
        ExhibitComponent,
        ExhibitsComponent,
        HomeComponent,
        LeetsComponent,
        NavMenuComponent,
        ProseComponent
    ],
    imports: [
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        BrowserAnimationsModule,
        ClipboardModule,
        HammerModule,
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent, pathMatch: 'full' },
            { path: 'leet', component: LeetsComponent },
            { path: 'about', component: AboutComponent }
        ], { scrollPositionRestoration: 'enabled' }),
        MarkdownModule.forRoot({
            loader: HttpClient,
            markedOptions: {
                provide: MarkedOptions,
                useValue: {
                    gfm: false
                }
            }
        }),
        NgbModule,
        NgxGalleryModule,
        ScullyLibModule
    ],
    providers: [AppConfigService, DataService, ExhibitService, {
        provide: APP_INITIALIZER, useFactory: initConfig, deps: [AppConfigService], multi: true
    }],
    bootstrap: [AppComponent]
})
export class AppModule { }
