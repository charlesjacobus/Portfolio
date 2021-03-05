import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

import { find, isNil } from 'lodash';

import { ILeet } from '../../models/leet';

import { ExhibitService } from '../../services/exhibit.service';

@Component({
    selector: 'leets-gallery',
    templateUrl: './leets.component.html',
    styleUrls: ['./leets.component.css']
})
export class LeetsComponent implements OnInit {
    private clipboardContent: string;

    public leet: ILeet;
    public leets: Array<ILeet>;
    public loading: boolean = false;

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
        this.clipboardContent = null;
        this.loading = true;

        // If a code is being supplied, then consider that it may be a leet we've already created
        if (!isNil(code)) {
            let match: ILeet = find(this.leets, function (l: ILeet) { return l.code === code; });
            if (!isNil(match)) {
                this.leet = match;

                this.loading = false;

                return;
            }
        }

        // Otherwise, create the leet
        this.exhibitService.fetchLeet(code)
            .subscribe((leet: ILeet) => {
                if (leet && leet.file) {
                    let objectURL = 'data:image/jpeg;base64,' + leet.file;
                    leet.safeUrl = this.sanitizer.bypassSecurityTrustUrl(objectURL);

                    leet.identifier = leet.code.substring(0, 9);

                    if (isNil(code)) {
                        this.leets.unshift(leet);
                        this.scrollToBottom();
                    }
                    this.leet = leet;
                }

                this.loading = false;
            });
    }

    public getLeetCode(): string {
        return isNil(this.leet) ? null : this.leet.code;
    }

    public getLeetIdentifier(): string {
        return isNil(this.leet) ? null : this.leet.identifier;
    }

    public isCurrentLeetOnClipboard(): boolean {
        return isNil(this.leet) ? false : this.leet.code === this.clipboardContent;
    }

    public isSelectedLeet(code: string): boolean {
        return isNil(this.leet) || isNil(code) ? false : this.leet.code === code;
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

    private scrollToBottom(): void {
        var scrollOptions = {
            top: 0,
            behavior: 'smooth'
        }
        this.leetsContainer.nativeElement.scrollTo(scrollOptions);
    }
}
