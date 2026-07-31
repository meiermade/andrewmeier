import * as cloudflare from '@pulumi/cloudflare'
import { tunnelHostname } from './tunnel'
import { provider } from './provider'
import { andymeierZone, andrewmeierZone } from './zone'
import * as config from '../config'

export const andymeier = new cloudflare.DnsRecord(config.identifier, {
    name: '@',
    zoneId: andymeierZone.id,
    type: 'CNAME',
    content: tunnelHostname,
    proxied: true,
    ttl: 1
}, { provider })

// DNS records for redirect domains — proxied A records pointing to a dummy IP
// so Cloudflare can intercept requests and apply redirect rulesets.

new cloudflare.DnsRecord(`${config.identifier}-andrewmeier-root`, {
    name: '@',
    zoneId: andrewmeierZone.id,
    type: 'A',
    content: '192.0.2.1',
    proxied: true,
    ttl: 1
}, { provider })

new cloudflare.DnsRecord(`${config.identifier}-andrewmeier-www`, {
    name: 'www',
    zoneId: andrewmeierZone.id,
    type: 'A',
    content: '192.0.2.1',
    proxied: true,
    ttl: 1
}, { provider })
