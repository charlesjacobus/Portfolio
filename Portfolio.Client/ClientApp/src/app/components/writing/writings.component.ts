import { AfterViewInit, Component, HostListener, OnInit, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { Router } from '@angular/router';

import { filter, findIndex, head, isArray, isEmpty, isNil, map } from 'lodash';

import { NodeItem } from '../tree-view/node-item';
import { TreeViewComponent } from '../tree-view/tree-view.component';

import { IWork } from '../../models/work';
import { IWriting } from '../../models/writing';
import { ExhibitService } from '../../services/exhibit.service';
import { WorkComponent } from '../exhibit/work.component';
import { WritingService } from '../../services/writing.service';

@Component({
    selector: 'writings',
    templateUrl: './writings.component.html',
    standalone: false,
    styleUrls: ['./writings.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager
})
export class WritingsComponent extends WorkComponent implements AfterViewInit, OnInit {
    private pageTitle: string = 'Writings';

    public errorFetchingWritingSummaries: boolean = false;
    public loading: boolean = true;
    public nodeItems: Array<NodeItem<IWriting>> = [];
    public selectedWork: IWork | null = null;

    private screenHeight!: number;
    private screenWidth!: number;
    private selectedWritingUrl: string | null = null;

    @ViewChild('toc') private toc!: TreeViewComponent;

    constructor(exhibitService: ExhibitService, private writingService: WritingService, protected override router: Router, private metaService: Meta, private titleService: Title)
    {
        super(exhibitService, router);

        this.onResize();
    }

    public override ngAfterViewInit(): void {
        super.ngAfterViewInit();

        // Upon initialization, the TreeViewComponent isn't accessible unless delayed
        if (!isNil(this.toc)) {
            const that = this;
            setTimeout(function () {
                that.toc.collapseAll();
            }, 500);
        }
    }

    @HostListener('window:resize', ['$event'])
    onResize(event? : any) {
        this.screenHeight = window.innerHeight;
        this.screenWidth = window.innerWidth;
    }

    public override getAssetsFolderName(): string {
        return 'writings';
    }

    public override getExhibitAnchor(): string {
        return isNil(this.selectedWork?.anchor) ? 'portfolio' : this.selectedWork.anchor;
    }

    public getSelectedWritingUrl(): string | null {
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
        this.selectedWork = head(selected) ?? null;
        if (isNil(this.selectedWork)) {
            return;
        }

        this.loadWriting(this.getFileFullName(this.selectedWork.fileName));

        // this.loading is false, once the markdown component reports that the selected writing is ready 
    }

    public handleSelectedWritingReady(): void {
        // Writing markdown is ready, so transition opacity
        this.loading = false;
    }

    protected initialize(): void {
        this.initializeContent();
        this.initializeMeta();
        this.initializeTitle();
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

    private initializeMeta(): void {
        const metaDescription = 'Read the writings of Charles Jacobus, including prose, poetry, and art criticism and theory';
        this.metaService.updateTag({ name: 'description', content: metaDescription });
        this.metaService.updateTag({ property: 'og:description', content: metaDescription });
    }

    private initializeTitle(): void {
        const currentTitle = this.titleService.getTitle();
        const baseName = currentTitle.split(':')[0].trim();
        this.titleService.setTitle(`${baseName} : ${this.pageTitle}`);
    }

    protected initializeToc(writings: Array<IWriting>): void {
        // Map writings to tree node items
        let nodeItems: Array<NodeItem<IWriting>> = map(writings, function (writing: IWriting) {
            let children: Array<any> | null = map(filter(writing.works, function (w: IWork) { return !isEmpty(w.name); }), function (work: IWork) {
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

        this.nodeItems = nodeItems;
    }

    protected loadWriting(fileName: string | null): void {
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
