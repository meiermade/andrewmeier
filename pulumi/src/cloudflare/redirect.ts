import * as cloudflare from '@pulumi/cloudflare'
import { provider } from './provider'
import { andrewmeierZone } from './zone'
import * as config from '../config'

// andrewmeier.dev -> andymeier.dev
new cloudflare.Ruleset(`${config.identifier}-andrewmeier-redirect`, {
    zoneId: andrewmeierZone.id,
    name: 'Redirect andrewmeier.dev to andymeier.dev',
    kind: 'zone',
    phase: 'http_request_dynamic_redirect',
    rules: [{
        ref: 'andrewmeier_to_andymeier',
        description: 'Redirect andrewmeier.dev and www.andrewmeier.dev to andymeier.dev preserving path',
        enabled: true,
        expression: '(http.host eq "andrewmeier.dev") or (http.host eq "www.andrewmeier.dev")',
        action: 'redirect',
        actionParameters: {
            fromValue: {
                statusCode: 301,
                preserveQueryString: true,
                targetUrl: {
                    expression: 'concat("https://andymeier.dev", http.request.uri.path)'
                }
            }
        }
    }]
}, { provider })
