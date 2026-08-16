import * as pulumi from '@pulumi/pulumi'
import * as path from 'path'

export const rootDir = path.dirname(path.dirname(__dirname))

export const identifier = 'andymeier'

const rawDockerConfig = new pulumi.Config('docker')

export const dockerConfig = {
    registryUri: rawDockerConfig.require('registryUri'),
    registryAccessToken: rawDockerConfig.requireSecret('registryAccessToken'),
}

const rawCloudflareConfig = new pulumi.Config('cloudflare')

export const cloudflareConfig = {
    accountId: rawCloudflareConfig.require('accountId'),
    apiToken: rawCloudflareConfig.requireSecret('apiToken'),
    cloudflaredVersion: '2026.7.3'
}

const rawK8sConfig = new pulumi.Config('k8s')

export const k8sConfig = {
    namespace: rawK8sConfig.require('namespace')
}

const rawOpenTelemetryConfig = new pulumi.Config('openTelemetry')

export const openTelemetryConfig = {
    endpoint: rawOpenTelemetryConfig.require('endpoint'),
    publicEndpoint: rawOpenTelemetryConfig.require('publicEndpoint'),
}
