using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Model.Storage.Unified
{
    public enum MultipartStatusEnum
    {
        NotSet,
        Initiated,
        ChunkUpload,
        Completed,
        Aborted
    }
}
