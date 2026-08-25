import * as cloudflare from '@pulumi/cloudflare'
import * as config from '../config'
import { provider } from './provider'
import { andymeierZone } from './zone'

new cloudflare.ZoneSetting(`${config.identifier}-ip-geolocation`, {
    zoneId: andymeierZone.id,
    settingId: 'ip_geolocation',
    value: 'on',
}, { provider })
