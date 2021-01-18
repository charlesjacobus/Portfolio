import { ScullyConfig } from '@scullyio/scully';

// IMPORTANT: staticPort must match the value of localhostWebPort in config.json in order for Scully to work
export const config: ScullyConfig = {
    projectRoot: "./src",
    projectName: "Portfolio.Client",
    outDir: './dist/static',
    routes: {},
    staticPort: 58195
};
