import * as hal from 'hr.halcyon.EndpointClient';

export class EntryPointInjector {
    private url: string;
    private fetcher: hal.Fetcher;
    private instancePromise: Promise<EntryPointResult>;

    constructor(url: string, fetcher: hal.Fetcher) {
        this.url = url;
        this.fetcher = fetcher;
    }

    public load(): Promise<EntryPointResult> {
        if (!this.instancePromise) {
            this.instancePromise = EntryPointResult.Load(this.url, this.fetcher);
        }

        return this.instancePromise;
    }
}

export class EntryPointResult {
    private client: hal.HalEndpointClient;

    public static Load(url: string, fetcher: hal.Fetcher): Promise<EntryPointResult> {
        return hal.HalEndpointClient.Load({
            href: url,
            method: "GET"
        }, fetcher)
            .then(c => {
                 return new EntryPointResult(c);
             });
            }

    constructor(client: hal.HalEndpointClient) {
        this.client = client;
    }

    private strongData: EntryPoint = undefined;
    public get data(): EntryPoint {
        this.strongData = this.strongData || this.client.GetData<EntryPoint>();
        return this.strongData;
    }

    public save(): Promise<void> {
        return this.client.LoadLink("Save").then(hal.makeVoid);
    }

    public canSave(): boolean {
        return this.client.HasLink("Save");
    }

    public linkForSave(): hal.HalLink {
        return this.client.GetLink("Save");
    }
}
//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v9.10.49.0 (Newtonsoft.Json v10.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------





export interface EntryPoint {
}

export interface HalEndpointDocQuery {
    includeRequest?: boolean;
    includeResponse?: boolean;
}
