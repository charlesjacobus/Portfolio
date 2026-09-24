export interface NodeItem<T> {
    id: string;
    name: string;
    item?: T;
    children?: Array<NodeItem<any>> | null;
    expanded?: boolean;
}
