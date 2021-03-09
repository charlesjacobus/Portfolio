import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

import { find, isNil } from 'lodash';

import { ILeet } from '../../models/leet';

import { ExhibitService } from '../../services/exhibit.service';

@Component({
    selector: 'leet-game',
    templateUrl: './leets.component.html',
    styleUrls: ['./leets.component.css']
})
export class LeetsComponent implements OnInit {
    private clipboardContent: string;
    private prefetch: ILeet;

    public leet: ILeet;
    public leets: Array<ILeet>;

    constructor(private exhibitService: ExhibitService, private sanitizer: DomSanitizer) { }

    @ViewChild('leetsContainer') private leetsContainer: ElementRef;

    public ngOnInit(): void {
        this.leets = [];

        this.getLeet();
    }

    public copiedToClipboard(event: any): void {
        this.clipboardContent = null;
        if (!isNil(event) && !isNil(event.content)) {
            this.clipboardContent = event.content;
        }
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

    public isSelectedLeet(code: string): boolean {
        return isNil(this.leet) || isNil(code) ? false : this.leet.code === code;
    }

    public leetIsCaptured(): boolean {
        if (isNil(this.leet) || isNil(this.clipboardContent)) {
            return false;
        }

        return this.leet.code === this.clipboardContent;
    }

    public sendEmail(): void {
        window.location.href = this.getEmailHref();
    }

    private getEmailHref(): string {
        if (isNil(this.leet)) {
            return null;
        }

        let body: string = 'I\'d like to order this leet: '.concat(this.leet.code);

        return 'mailto:charleshenryjacobus@gmail.com?subject=Leet&body='.concat(body);
    }

    private scrollToTop(): void {
        var scrollOptions = {
            top: 0,
            behavior: 'smooth'
        }
        this.leetsContainer.nativeElement.scrollTo(scrollOptions);
    }
}
