import { Component, Inject, OnInit, OnDestroy, PLATFORM_ID } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { isPlatformBrowser, DOCUMENT } from '@angular/common';
import { Subscription } from 'rxjs';

import { fadeAnimation } from './animations';
import { AppConfigService } from './services/config.service';

@Component({
    selector: 'app-root',
    standalone: false,
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css'],
    animations: [fadeAnimation]
})
export class AppComponent implements OnInit, OnDestroy {
    private routerSub?: Subscription;

    constructor(
        private router: Router,
        @Inject(DOCUMENT) private document: Document,
        @Inject(PLATFORM_ID) private platformId: Object,
        private config: AppConfigService
    ) { }

    ngOnInit(): void {
        this.initializeCanonical();
    }

    ngOnDestroy(): void {
        if (this.routerSub) {
            this.routerSub.unsubscribe();
            this.routerSub = undefined;
        }
    }

    private initializeCanonical(): void {
        if (!isPlatformBrowser(this.platformId)) {
            return;
        }

        this.routerSub = this.router.events.subscribe(event => {
            if (!(event instanceof NavigationEnd)) {
                return;
            }

            let link = this.document.querySelector<HTMLLinkElement>('link[rel="canonical"]');
            if (!link) {
                link = this.document.createElement('link');
                link.setAttribute('rel', 'canonical');
                this.document.head.appendChild(link);
            }

            const configuredHost = (this.config as any).configuration?.hostWeb;
            const origin = configuredHost ? `https://${configuredHost}` : (this.document.location?.origin ?? '');
            const path = event.urlAfterRedirects.startsWith('/') ? event.urlAfterRedirects : `/${event.urlAfterRedirects}`;
            const href = `${origin}${path}`;

            link.setAttribute('href', href);
        });
    }
}
