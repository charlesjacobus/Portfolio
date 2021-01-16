export interface IConfig {
  apiUseSsl: boolean;
  apiUrl: string;
  apiInfoPath: string;
  explicitPerEnvironmentConfiguration: boolean;
  hostApi: string;
  hostWeb: string;
  localhostApiPort: number;
  localhostWebPort: number;
}
