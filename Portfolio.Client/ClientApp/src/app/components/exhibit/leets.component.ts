import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { Router } from '@angular/router';

import { find, isNil } from 'lodash';

import { ILeet } from '../../models/leet';
import { ExhibitService } from '../../services/exhibit.service';
import { WorkComponent } from './work.component';

@Component({
    selector: 'leet-game',
    templateUrl: './leets.component.html',
    styleUrls: ['./leets.component.css']
})
export class LeetsComponent extends WorkComponent implements OnInit {
    private static readonly LeetName = 'Leet';

    private clipboardContent: string;
    private prefetch: ILeet;

    public instructions: boolean = false;
    public leet: ILeet;
    public leets: Array<ILeet>;

    constructor(protected exhibitService: ExhibitService, protected router: Router, private sanitizer: DomSanitizer) {
        super(exhibitService, router);
    }

    @ViewChild('leetsContainer') private leetsContainer: ElementRef;

    public ngOnInit(): void {
        this.leets = [];

        this.getLeet();

        this.initialize();
    }

    public copiedToClipboard(event: any): void {
        this.clipboardContent = null;
        if (!isNil(event) && !isNil(event.content)) {
            this.clipboardContent = event.content;
        }
    }

    public getCurrentLeet(): ILeet {
        return this.leet;
    }

    public getLeet(code: string = null): void {
        // If a code is supplied, which it is as the user clicks through the list of leets already created, then the existing leet (if found) is returned
        // Leets are prefetched one at a time, to improve the UX when clicking Create Cartoon
        // This means that the prefetched leet is unshifted to the collection and a new leet is fetched in reserve
        
        // If a code is being supplied, then consider that it may be a leet we've already created
        if (!isNil(code)) {
            let match: ILeet = find(this.leets, function (l: ILeet) { return l.code === code; });
            if (!isNil(match)) {
                this.leet = match;

                return;
            }
        }

        // If there's a prefetch, use it
        let refetch = isNil(this.prefetch);
        if (!isNil(this.prefetch)) {
            this.leets.unshift(this.prefetch);
            this.scrollToTop();

            this.leet = this.prefetch;
            this.prefetch = null;
        }

        // Otherwise, create a leet and reset the prefetch
        this.exhibitService.fetchLeet(code)
            .subscribe((leet: ILeet) => {
                if (leet && leet.file) {
                    let objectURL = 'data:image/jpeg;base64,' + leet.file;
                    leet.safeUrl = this.sanitizer.bypassSecurityTrustUrl(objectURL);

                    leet.identifier = leet.code.substring(0, 9);

                    this.prefetch = leet;

                    if (refetch) {
                        this.getLeet();
                    }
                }
            });
    }

    public getLeetCode(): string {
        return isNil(this.leet) ? null : this.leet.code;
    }

    public getLeets(): Array<ILeet> {
        return this.leets;
    }

    public isSelectedLeet(code: string): boolean {
        return isNil(this.leet) || isNil(code) ? false : this.leet.code === code;
    }

    public leetIsCaptured(): boolean {
        if (isNil(this.leet) || isNil(this.clipboardContent)) {
            return false;
        }

        return this.leet.code === this.clipboardContent;
    }

    public toggleInstructions(): void {
        this.instructions = !this.instructions;
    }

    public sendEmail(): void {
        window.location.href = this.getEmailHref();
    }

    protected initialize(): void {
        this.exhibitSummary = {
            id: 11,
            anchor: LeetsComponent.LeetName.toLowerCase(),
            description: null,
            descriptionFileName: LeetsComponent.LeetName.concat('.md'),
            name: LeetsComponent.LeetName,
            promo: null,
            textIsDefault: false,
            textLabel: null,
            textRoute: null
        };
    }

    private getEmailHref(): string {
        if (isNil(this.leet)) {
            return null;
        }

        let body: string = 'I\'d like to order this leet! '.concat(this.leet.code);

        return 'mailto:charleshenryjacobus@gmail.com?subject=Leet&body='.concat(body);
    }

    private scrollToTop(): void {
        if (isNil(this.leetsContainer) || isNil(this.leetsContainer.nativeElement)) {
            return;
        }

        var scrollOptions = {
            top: 0,
            behavior: 'smooth'
        }
        this.leetsContainer.nativeElement.scrollTo(scrollOptions);
    }
}
