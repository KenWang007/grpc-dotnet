﻿#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client.Internal;
using Grpc.Net.Compression;
using Grpc.Shared;
using Microsoft.Extensions.Logging;

namespace Grpc.Net.Client.Tests.Infrastructure
{
    internal static class StreamSerializationHelper
    {
        public static ValueTask<TResponse?> ReadMessageAsync<TResponse>(
            Stream responseStream,
            //ILogger logger,
            Func<DeserializationContext, TResponse> deserializer,
            string grpcEncoding,
            int? maximumMessageSize,
            Dictionary<string, ICompressionProvider> compressionProviders,
            bool singleMessage,
            CancellationToken cancellationToken)
            where TResponse : class
        {
            var tempChannel = GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = maximumMessageSize,
                CompressionProviders = compressionProviders?.Values.ToList()
            });

            var tempCall = new TestGrpcCall(new CallOptions(), tempChannel, typeof(TResponse));

            return responseStream.ReadMessageAsync(tempCall, deserializer, grpcEncoding, singleMessage, cancellationToken);
        }

        private class TestGrpcCall : GrpcCall
        {
            private readonly Type _type;

            public TestGrpcCall(CallOptions options, GrpcChannel channel, Type type) : base(options, channel)
            {
                _type = type;
            }

            public override Type RequestType => _type;
            public override Type ResponseType => _type;
        }
    }
}
