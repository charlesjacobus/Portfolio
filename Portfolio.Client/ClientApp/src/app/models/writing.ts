import { IWork } from "./work";

export interface IWriting {
    id: number;
    name: string;
    anchor: string;
    fileName: string;
    works: Array<IWork>;
}
