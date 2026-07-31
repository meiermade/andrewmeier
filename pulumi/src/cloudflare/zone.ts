import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import * as config from '../config'

const getZone = (name: string) =>
    cloudflare.getZoneOutput({
        filter: {
            account: {
                id: config.cloudflareConfig.accountId
            },
            name
        }
    }, { provider })

export const andymeierZone = getZone('andymeier.dev')
export const andrewmeierZone = getZone('andrewmeier.dev')
