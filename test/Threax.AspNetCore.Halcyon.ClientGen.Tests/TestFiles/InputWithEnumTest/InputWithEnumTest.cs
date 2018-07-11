using Threax.AspNetCore.Halcyon.Client;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;

namespace Test {

public class OutputResult 
{
    private HalEndpointClient client;

    public OutputResult(HalEndpointClient client) 
    {
        this.client = client;
    }

    private Output strongData = default(Output);
    public Output Data 
    {
        get
        {
            if(this.strongData == default(Output))
            {
                this.strongData = this.client.GetData<Output>();  
            }
            return this.strongData;
        }
    }

    public async Task Save(Input data) 
    {
        var result = await this.client.LoadLinkWithData("Save", data);
    }

    public bool CanSave 
    {
        get 
        {
            return this.client.HasLink("Save");
        }
    }

    public HalLink LinkForSave 
    {
        get 
        {
            return this.client.GetLink("Save");
        }
    }

    public async Task<HalEndpointDoc> GetSaveDocs() 
    {
        var result = await this.client.LoadLinkDoc("Save");
        return result.GetData<HalEndpointDoc>();
    }

    public bool HasSaveDocs() {
        return this.client.HasLinkDoc("Save");
    }
}

public class OutputResult 
{
    private HalEndpointClient client;

    public OutputResult(HalEndpointClient client) 
    {
        this.client = client;
    }

    private Output strongData = default(Output);
    public Output Data 
    {
        get
        {
            if(this.strongData == default(Output))
            {
                this.strongData = this.client.GetData<Output>();  
            }
            return this.strongData;
        }
    }

    public async Task Save(AnotherInput data) 
    {
        var result = await this.client.LoadLinkWithData("Save", data);
    }

    public bool CanSave 
    {
        get 
        {
            return this.client.HasLink("Save");
        }
    }

    public HalLink LinkForSave 
    {
        get 
        {
            return this.client.GetLink("Save");
        }
    }

    public async Task<HalEndpointDoc> GetSaveDocs() 
    {
        var result = await this.client.LoadLinkDoc("Save");
        return result.GetData<HalEndpointDoc>();
    }

    public bool HasSaveDocs() {
        return this.client.HasLinkDoc("Save");
    }
}
}
//----------------------
// <auto-generated>
//     Generated using the NJsonSchema v9.10.49.0 (Newtonsoft.Json v10.0.0.0) (http://NJsonSchema.org)
// </auto-generated>
//----------------------

namespace Test
{
    #pragma warning disable // Disable all warnings

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.10.49.0 (Newtonsoft.Json v10.0.0.0)")]
    public enum TestEnum
    {
        [System.Runtime.Serialization.EnumMember(Value = "One")]
        One = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = "Two")]
        Two = 1,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.10.49.0 (Newtonsoft.Json v10.0.0.0)")]
    public partial class Output 
    {
        [Newtonsoft.Json.JsonProperty("enumValue", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public TestEnum EnumValue { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static Output FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Output>(data);
        }
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.10.49.0 (Newtonsoft.Json v10.0.0.0)")]
    public partial class Input 
    {
        [Newtonsoft.Json.JsonProperty("enumValue", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public TestEnum EnumValue { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static Input FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Input>(data);
        }
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "9.10.49.0 (Newtonsoft.Json v10.0.0.0)")]
    public partial class AnotherInput 
    {
        [Newtonsoft.Json.JsonProperty("enumValue", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public TestEnum EnumValue { get; set; }
    
        public string ToJson() 
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        
        public static AnotherInput FromJson(string data)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AnotherInput>(data);
        }
    
    }
}
