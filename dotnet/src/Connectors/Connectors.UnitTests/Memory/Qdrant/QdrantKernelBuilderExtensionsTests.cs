﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Xunit;

namespace SemanticKernel.Connectors.UnitTests.Memory.Qdrant;

public sealed class QdrantKernelBuilderExtensionsTests : IDisposable
{
    private readonly HttpMessageHandlerStub _messageHandlerStub;
    private readonly HttpClient _httpClient;

    public QdrantKernelBuilderExtensionsTests()
    {
        this._messageHandlerStub = new HttpMessageHandlerStub();

        this._httpClient = new HttpClient(this._messageHandlerStub, false);
    }

    [Fact]
    public async Task QdrantMemoryStoreShouldBeProperlyInitializedAsync()
    {
        //Arrange
        this._httpClient.BaseAddress = new Uri("https://fake-random-qdrant-host");
        this._messageHandlerStub.ResponseToReturn.Content = new StringContent("{\"result\":{\"collections\":[]}}", Encoding.UTF8, MediaTypeNames.Application.Json);

        var builder = new KernelBuilder();
        builder.WithQdrantMemoryStore(this._httpClient, 123);
        builder.WithAzureTextEmbeddingGenerationService("fake-deployment-name", "https://fake-random-text-embedding-generation-host/fake-path", "fake-api-key");
        var kernel = builder.Build(); //This call triggers the internal factory registered by WithQdrantMemoryStore method to create an instance of the QdrantMemoryStore class.

        //Act
        await kernel.Memory.GetCollectionsAsync(); //This call triggers a subsequent call to Qdrant memory store.

        //Assert
        Assert.Equal("https://fake-random-qdrant-host/collections", this._messageHandlerStub?.RequestUri?.AbsoluteUri);
    }

    public void Dispose()
    {
        this._httpClient.Dispose();
        this._messageHandlerStub.Dispose();
    }
}
