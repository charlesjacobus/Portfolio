import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';

import { NodeItem } from './node-item';

export interface TreeViewNodeState<T> {
    nodeItem: NodeItem<T>;
    expanded: boolean;
    selected: boolean;
    children: Array<TreeViewNodeState<any>>;
}

// A minimal, single-select-only reimplementation of the parts of the unmaintained 'tree-ngx'
// package this site actually uses. Kept intentionally small: only collapseAll() and single
// selection with toggle-off (matching tree-ngx's SingleSelect + alwaysEmitSelected behavior)
// are implemented, since those are the only behaviors any caller exercises.
@Component({
    selector: 'tree-view',
    standalone: false,
    templateUrl: './tree-view.component.html',
    styleUrls: ['./tree-view.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager
})
export class TreeViewComponent implements OnChanges, OnInit {
    @Input() nodeItems: Array<NodeItem<any>> = [];
    @Output() selectedItems = new EventEmitter<Array<any>>();

    public rootStates: Array<TreeViewNodeState<any>> = [];

    private selectedState: TreeViewNodeState<any> | null = null;

    public ngOnInit(): void {
        // tree-ngx exposes selection through a BehaviorSubject, which always emits its (initially
        // empty) current value as soon as something subscribes. The writings view relies on that
        // initial empty emission to load its default content before anything has been clicked.
        setTimeout(() => this.selectedItems.emit([]), 0);
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes.nodeItems) {
            this.rootStates = (this.nodeItems || []).map(nodeItem => this.buildState(nodeItem));
            this.selectedState = null;
        }
    }

    public collapseAll(): void {
        this.setExpandedRecursive(this.rootStates, false);
    }

    public handleNameClick(state: TreeViewNodeState<any>): void {
        if (state === this.selectedState) {
            state.selected = false;
            this.selectedState = null;
            this.selectedItems.emit([]);

            return;
        }

        if (this.selectedState) {
            this.selectedState.selected = false;
        }

        state.selected = true;
        this.selectedState = state;
        this.selectedItems.emit([state.nodeItem.item]);
    }

    private buildState(nodeItem: NodeItem<any>): TreeViewNodeState<any> {
        return {
            nodeItem,
            expanded: nodeItem.expanded !== false,
            selected: false,
            children: (nodeItem.children || []).map(child => this.buildState(child))
        };
    }

    private setExpandedRecursive(states: Array<TreeViewNodeState<any>>, expanded: boolean): void {
        for (const state of states) {
            state.expanded = expanded;
            this.setExpandedRecursive(state.children, expanded);
        }
    }
}
