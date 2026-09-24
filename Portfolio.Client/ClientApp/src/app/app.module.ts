import { NgModule, inject, provideAppInitializer, provideZoneChangeDetection, SecurityContext } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

import { AngularSplitModule } from 'angular-split';
import { ClipboardModule } from 'ngx-clipboard';
import { MarkdownModule, MARKED_OPTIONS, SANITIZE } from 'ngx-markdown';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';

import { AboutComponent } from './components/about/about.component';
import { AppComponent } from './app.component';
import { CopyrightComponent } from './components/copyright/copyright.component';
import { ExhibitComponent } from './components/exhibit/exhibit.component';
import { ExhibitsComponent } from './components/exhibit/exhibits.component';
import { HomeComponent } from './components/home/home.component';
import { LeetsComponent } from './components/exhibit/leets.component';
import { NavMenuComponent } from './components/nav-menu/nav-menu.component';
import { TreeViewComponent } from './components/tree-view/tree-view.component';
import { TreeViewNodeComponent } from './components/tree-view/tree-view-node.component';
import { WritingsComponent } from './components/writing/writings.component';

import { AppConfigService } from './services/config.service';
import { DataService } from './services/data.service';
import { ExhibitService } from './services/exhibit.service';
import { WritingService } from './services/writing.service';

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
        TreeViewComponent,
        TreeViewNodeComponent,
        WritingsComponent
    ],
    imports: [
        AngularSplitModule,
        BrowserModule,
        ClipboardModule,
        FormsModule,
        RouterModule.forRoot([
            { path: '', component: HomeComponent },
            { path: 'exhibits', component: ExhibitsComponent },
            { path: 'exhibits/:exhibitIdentifier', component: ExhibitsComponent },
            { path: 'leet', component: LeetsComponent },
            { path: 'about', component: AboutComponent },
            { path: 'writings', component: WritingsComponent },
            { path: '**', redirectTo: '', pathMatch: 'full' }
        ], {
            anchorScrolling: 'enabled',
            scrollPositionRestoration: 'enabled'
        }),
        MarkdownModule.forRoot({
            loader: HttpClient,
            markedOptions: {
                provide: MARKED_OPTIONS,
                useValue: {
                    gfm: false
                }
            },
            sanitize: {
                provide: SANITIZE,
                useValue: SecurityContext.NONE
            }
        }),
        NgbModule
    ],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideZoneChangeDetection({
            eventCoalescing: true
        }),
        AppConfigService,
        DataService,
        ExhibitService,
        WritingService,
        provideAppInitializer(() => inject(AppConfigService).load())
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }
