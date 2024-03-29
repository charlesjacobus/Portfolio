import { BrowserModule } from '@angular/platform-browser';
import { HammerModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule, APP_INITIALIZER, SecurityContext } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AngularSplitModule } from 'angular-split';
import { ClipboardModule } from 'ngx-clipboard';
import { MarkdownModule, MarkedOptions } from 'ngx-markdown';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxGalleryModule } from 'ngx-gallery-9';
import { TreeNgxModule } from 'tree-ngx';

import { AboutComponent } from './components/about/about.component';
import { AppComponent } from './app.component';
import { CopyrightComponent } from './components/copyright/copyright.component';
import { ExhibitComponent } from './components/exhibit/exhibit.component';
import { ExhibitsComponent } from './components/exhibit/exhibits.component';
import { HomeComponent } from './components/home/home.component';
import { LeetsComponent } from './components/exhibit/leets.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { WritingsComponent } from './components/writing/writings.component';

import { AppConfigService } from './services/config.service';
import { DataService } from './services/data.service';
import { ExhibitService } from './services/exhibit.service';
import { WritingService } from './services/writing.service';

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
        WritingsComponent
    ],
    imports: [
        AngularSplitModule,
        BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
        BrowserAnimationsModule,
        ClipboardModule,
        HammerModule,
        HttpClientModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent },
            { path: 'exhibits', component: ExhibitsComponent },
            { path: 'exhibits/:exhibitIdentifier', component: ExhibitsComponent },
            { path: 'leet', component: LeetsComponent },
            { path: 'about', component: AboutComponent },
            { path: 'writings', component: WritingsComponent },
            { path: '**', redirectTo: '', pathMatch: 'full' }],
            {
                anchorScrolling: 'enabled',
                scrollPositionRestoration: 'enabled'
            }),
        MarkdownModule.forRoot({
            loader: HttpClient,
            markedOptions: {
                provide: MarkedOptions,
                useValue: {
                    gfm: false
                }
            },
            sanitize: SecurityContext.NONE
        }),
        NgbModule,
        NgxGalleryModule,
        TreeNgxModule
    ],
    providers: [AppConfigService, DataService, ExhibitService, WritingService, {
        provide: APP_INITIALIZER, useFactory: initConfig, deps: [AppConfigService], multi: true
    }],
    bootstrap: [AppComponent]
})
export class AppModule { }
