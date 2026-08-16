module App.Consent

open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open System
open System.Text.Json

[<Literal>]
let cookieName = "analytics-consent"

[<Literal>]
let private policyVersion = "2026-08-16"

[<CLIMutable>]
type ConsentRequest = { analytics:string }

type Choice =
    | Accepted
    | Declined

let private tryChoice value =
    match value with
    | "accepted" -> Some Accepted
    | "declined" -> Some Declined
    | _ -> None

let private choiceValue choice =
    match choice with
    | Accepted -> "accepted"
    | Declined -> "declined"

let private appendCookie (ctx:HttpContext) choice =
    let options = CookieOptions()
    options.HttpOnly <- false
    options.IsEssential <- true
    options.MaxAge <- Nullable(TimeSpan.FromDays 180.)
    options.Path <- "/"
    options.SameSite <- SameSiteMode.Lax
    let environment = ctx.GetService<IHostEnvironment>()
    options.Secure <- not (environment.IsDevelopment())

    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    let value = $"v1.{choiceValue choice}.{policyVersion}.{timestamp}"
    ctx.Response.Cookies.Append(cookieName, value, options)

let persist : HttpHandler =
    fun next ctx -> task {
        try
            let! request = ctx.BindJsonAsync<ConsentRequest>()
            match tryChoice request.analytics with
            | Some choice ->
                appendCookie ctx choice
                ctx.SetStatusCode StatusCodes.Status204NoContent
                return Some ctx
            | None ->
                return! RequestErrors.BAD_REQUEST "Invalid analytics consent choice." next ctx
        with :? JsonException ->
            return! RequestErrors.BAD_REQUEST "Invalid consent request." next ctx
    }

let clientScript = """
window.analyticsConsentChoice=window.analyticsConsentChoice||function(){
  var item=document.cookie.split(';').map(function(value){return value.trim();}).find(function(value){return value.indexOf('analytics-consent=')===0;});
  if(!item)return null;
  var match=/^v1\.(accepted|declined)\.\d{4}-\d{2}-\d{2}\.\d+$/.exec(decodeURIComponent(item.substring('analytics-consent='.length)));
  return match?match[1]:null;
};
window.applyAnalyticsConsent=window.applyAnalyticsConsent||async function(value){
  if(value==='accepted'){
    if(window.loadOpenTelemetry)await window.loadOpenTelemetry();
  }else if(window.disableOpenTelemetry){
    await window.disableOpenTelemetry();
  }
};
window.persistAnalyticsConsent=window.persistAnalyticsConsent||async function(value){
  var response=await fetch('/privacy/consent',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({analytics:value})});
  if(!response.ok)throw new Error('Unable to save analytics preference.');
  localStorage.removeItem('analytics-consent');
};
window.setAnalyticsConsent=window.setAnalyticsConsent||async function(value){
  var banner=document.getElementById('cookie-consent-banner');
  var error=document.getElementById('analytics-consent-error');
  var buttons=banner?banner.querySelectorAll('button'):[];
  buttons.forEach(function(button){button.disabled=true;});
  if(error)error.classList.add('hidden');
  if(value==='declined')await window.applyAnalyticsConsent(value);
  try{
    await window.persistAnalyticsConsent(value);
    if(value==='accepted')await window.applyAnalyticsConsent(value);
    if(banner)banner.classList.add('hidden');
  }catch(_error){
    document.cookie='analytics-consent=; Max-Age=0; Path=/; SameSite=Lax';
    if(error){error.textContent='We could not save your analytics preference. Please try again.';error.classList.remove('hidden');}
    if(banner)banner.classList.remove('hidden');
  }finally{
    buttons.forEach(function(button){button.disabled=false;});
  }
};
window.showAnalyticsSettings=window.showAnalyticsSettings||function(){
  var banner=document.getElementById('cookie-consent-banner');
  if(banner)banner.classList.remove('hidden');
};
document.addEventListener('DOMContentLoaded',function(){
  var saved=window.analyticsConsentChoice();
  var legacy=localStorage.getItem('analytics-consent');
  if(saved==='accepted'||saved==='declined'){
    window.applyAnalyticsConsent(saved);
    var banner=document.getElementById('cookie-consent-banner');
    if(banner)banner.classList.add('hidden');
  }else if(legacy==='accepted'||legacy==='declined'){
    window.setAnalyticsConsent(legacy);
  }else{
    window.showAnalyticsSettings();
  }
  var accept=document.getElementById('analytics-accept');
  var reject=document.getElementById('analytics-reject');
  var settings=document.getElementById('analytics-settings');
  if(accept)accept.addEventListener('click',function(){window.setAnalyticsConsent('accepted');});
  if(reject)reject.addEventListener('click',function(){window.setAnalyticsConsent('declined');});
  if(settings)settings.addEventListener('click',window.showAnalyticsSettings);
});
"""
