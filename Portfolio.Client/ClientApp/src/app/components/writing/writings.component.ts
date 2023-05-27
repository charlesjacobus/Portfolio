import { trigger, state, style, animate, transition } from '@angular/animations';
import { AfterViewInit, Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';

import { filter, findIndex, head, isArray, isEmpty, isNil, map } from 'lodash';

import { NodeItem, TreeMode, TreeNgxComponent, TreeOptions } from 'tree-ngx';

import { IWork } from '../../models/work';
import { IWriting } from '../../models/writing';
import { ExhibitService } from '../../services/exhibit.service';
import { WorkComponent } from '../exhibit/work.component';
import { WritingService } from '../../services/writing.service';

@Component({
    selector: 'writings',
    templateUrl: './writings.component.html',
    styleUrls: ['./writings.component.css'],
    animations: [
        trigger('writingLoading', [
            state('unload', style({
                opacity: 1
            })),
            state('load', style({
                opacity: 0
            })),
            transition('load => unload', animate('200ms ease-out')),
            transition('unload => load', animate('200ms ease-in'))
        ])
    ]
})
export class WritingsComponent extends WorkComponent implements AfterViewInit, OnInit {
    public errorFetchingWritingSummaries: boolean = false;
    public loading: boolean = true;
    public nodeItems: Array<NodeItem<IWriting>> = [];
    public selectedWork: IWork = null;
    public treeOptions: TreeOptions = { alwaysEmitSelected: true, checkboxes: false, mode: TreeMode.SingleSelect };

    private defaultTocItemId: string = null;
    private screenHeight: number;
    private screenWidth: number;
    private selectedWritingUrl: string = null;

    @ViewChild('toc') private toc: TreeNgxComponent;

    constructor(exhibitService: ExhibitService, private writingService: WritingService, protected router: Router)
    {
        super(exhibitService, router);

        this.onResize();
    }

    public ngAfterViewInit(): void {
        super.ngAfterViewInit();

        // Upon initialization, the TreeNgxComponent isn't accessible unless delayed
        if (!isNil(this.toc)) {
            const that = this;
            setTimeout(function () {
                that.toc.collapseAll();

                if (!isNil(that.defaultTocItemId)) {
                    that.toc.expandById(that.defaultTocItemId);
                }
            }, 500);
        }
    }

    @HostListener('window:resize', ['$event'])
    onResize(event?) {
        this.screenHeight = window.innerHeight;
        this.screenWidth = window.innerWidth;
    }

    public getAssetsFolderName(): string {
        return 'writings';
    }

    public getExhibitAnchor(): string {
        return isNil(this.selectedWork) ? 'portfolio' : this.selectedWork.anchor;
    }

    public getLoadingName() {
        return this.loading ? 'load' : 'unload';
    }

    public getSelectedWritingUrl(): string {
        return this.selectedWritingUrl;
    }

    public getTocOrientation(): string {
        return this.screenWidth <= 800 ? 'vertical' : 'horizontal';
    }

    public handleSelectedTreeItems(selected: Array<IWork>): void {
        if (!isArray(selected) || selected.length <= 0 || isNil(selected[0])) {
            this.initializeContent();

            return;
        }

        this.loading = true;
        this.selectedWork = head(selected);

        this.loadWriting(this.getFileFullName(this.selectedWork.fileName));

        // this.loading is false, once the markdown component reports that the selected writing is ready 
    }

    public handleSelectedWritingReady(): void {
        // Writing markdown is ready, so transition opacity
        this.loading = false;
    }

    protected initialize(): void {
        this.initializeContent();
        this.writingService.fetchWritings()
            .subscribe({
                next: (writings: Array<IWriting>) => {
                    this.initializeToc(writings);
                },
                error: () => { this.errorFetchingWritingSummaries = true; },
                complete: () => { }
            });
    }

    protected initializeContent(): void {
        this.loading = true;
        this.selectedWork = null;

        this.loadWriting(this.getFileFullName('writings.md'));
    }

    protected initializeToc(writings: Array<IWriting>): void {
        // Map writings to tree node items
        let nodeItems: Array<NodeItem<IWriting>> = map(writings, function (writing: IWriting) {
            let children: Array<any> = map(filter(writing.works, function (w: IWork) { return !isEmpty(w.name); }), function (work: IWork) {
                return {
                    id: (findIndex(writing.works, function (w: IWork) { return w.fileName === work.fileName; }) + 1).toString(),
                    name: work.name,
                    anchor: writing.anchor,
                    item: {
                        anchor: writing.anchor,
                        fileName: work.fileName,
                        name: work.name
                    }
                }
            });

            // If there are no children, then ensure the children property is null
            // This is important to ensure that singleton node click handling works correctly
            if (!isArray(children) || children.length <= 0) {
                children = null;
            }

            // If there are no children, but there are children defined (without a name, so not visible), then attach the fileName of the first and only child to the parent
            if (isNil(children) && isArray(writing.works) && writing.works.length === 1) {
                writing.fileName = writing.works[0].fileName;
            }

            return {
                id: writing.id.toString(),
                name: writing.name,
                item: writing,
                children: children
            };
        });

        // If there are any node items, then the default item ID is the first/top item
        // This may not always be item #1 (tree node IDs are expected to be string values)
        // And it will ensure that the first/top item is auto-expanded upon initialization
        if (isArray(nodeItems) && nodeItems.length > 0) {
            // this.defaultTocItemId = head(nodeItems).id.toString();
        }

        this.nodeItems = nodeItems;
    }

    protected loadWriting(fileName: string): void {
        if (isEmpty(fileName)) {
            return;
        }

        // Defer the loading a bit, to support the transition animation
        const that = this;
        setTimeout(function () {
            that.selectedWritingUrl = fileName;
        }, 200);
    }
}
