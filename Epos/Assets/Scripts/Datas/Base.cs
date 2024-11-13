using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;

namespace Datas
{
    public class Base
    {
        [JsonProperty("id")] 
        public int Id { get; private set; } = 0;
    }
}

