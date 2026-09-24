import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

import { TreeViewComponent, TreeViewNodeState } from './tree-view.component';

@Component({
    selector: 'tree-view-node',
    standalone: false,
    templateUrl: './tree-view-node.component.html',
    styleUrls: ['./tree-view.component.css'],
    changeDetection: ChangeDetectionStrategy.Eager
})
export class TreeViewNodeComponent {
    @Input() state!: TreeViewNodeState<any>;
    @Input() tree!: TreeViewComponent;

    public get hasChildren(): boolean {
        return !!this.state.children && this.state.children.length > 0;
    }

    public toggleExpand(): void {
        if (this.hasChildren) {
            this.state.expanded = !this.state.expanded;
        }
    }

    public nameClick(): void {
        this.tree.handleNameClick(this.state);
    }
}
