(window.webpackJsonp=window.webpackJsonp||[]).push([[7],{967:function(e,t,a){"use strict";a.r(t),a.d(t,"_frontmatter",(function(){return i})),a.d(t,"default",(function(){return u}));a(11),a(6),a(5),a(3),a(7),a(4),a(8),a(1);var r=a(78),n=a(959);function o(){return(o=Object.assign||function(e){for(var t=1;t<arguments.length;t++){var a=arguments[t];for(var r in a)Object.prototype.hasOwnProperty.call(a,r)&&(e[r]=a[r])}return e}).apply(this,arguments)}var i={};void 0!==i&&i&&i===Object(i)&&Object.isExtensible(i)&&Object.defineProperty(i,"__filemeta",{configurable:!0,value:{name:"_frontmatter",filename:"src/pages/Deployment/Using_Spark.md"}});var s={_frontmatter:i},l=n.a;function u(e){var t=e.components,a=function(e,t){if(null==e)return{};var a,r,n={},o=Object.keys(e);for(r=0;r<o.length;r++)a=o[r],t.indexOf(a)>=0||(n[a]=e[a]);return n}(e,["components"]);return Object(r.b)(l,o({},s,a,{components:t,mdxType:"MDXLayout"}),Object(r.b)("p",null,"There are essentially two ways in which you can fit Spark in your own environment. One is using Spark as-is, the other requires you to modify the storage layer."),Object(r.b)("h2",{id:"using-spark-as-is"},"Using Spark as-is"),Object(r.b)("p",null,"In this mode, Spark handles everything FHIR for you, but it requires it's own storage of resources and the search index to do so. This means you have to feed the data that you want to serve as FHIR Resources into the Spark REST interface. So you create a copy of (part of) your data, held in the Spark MongoDB database. These are roughly the steps to follow:"),Object(r.b)("ul",null,Object(r.b)("li",{parentName:"ul"},"Define how the data in your own system(s) maps to FHIR resources. This is the logical mapping. "),Object(r.b)("li",{parentName:"ul"},"Create a piece of software that:\n",Object(r.b)("em",{parentName:"li"}," performs this logical mapping on actual data\n")," uploads the result of it to Spark (with a FHIR Create operation on the POST ",Object(r.b)("inlineCode",{parentName:"li"},"[base]/fhir/<resourcetype>")," endpoint)\nThe FhirClient from the Hl7.Fhir API is very useful to program this."),Object(r.b)("li",{parentName:"ul"},"If you need periodic updates from your system to Spark, run the software from the previous step periodically by any means you see fit. It is advised that only updates since the previous run can be recognized in your data, and you feed them into Spark instead of reloading all the data every time.")),Object(r.b)("h2",{id:"using-spark-directly-against-your-own-datastore"},"Using Spark directly against your own datastore"),Object(r.b)("p",null,"In this mode, Spark presents the FHIR REST interface to the outside world, but data retrieval (and eventually storage) is handled by an existing datastore. This requires quite some work in adapting Spark, because you have to replace Spark.Mongo with an implementation targeting your own datastore. Depending on the FHIR interactions that you intend to support, this may or may not be feasible. We think this is a valid option if you:"),Object(r.b)("ul",null,Object(r.b)("li",{parentName:"ul"},"provide read-only access"),Object(r.b)("li",{parentName:"ul"},"to a limited number of Resource types"),Object(r.b)("li",{parentName:"ul"},"supporting only a few search parameters")),Object(r.b)("p",null,"Please be aware that:"),Object(r.b)("ul",null,Object(r.b)("li",{parentName:"ul"},"A FHIR Resource has a server-assigned id, by which it can be retrieved again. Because a resource may span several datastructures in your own datastore, management of these id's is not always straightforward."),Object(r.b)("li",{parentName:"ul"},"A search must be translated to a (usually SQL-) query into your own datastore. Therefore, supporting many search parameters will probably require a lot of work.")),Object(r.b)("p",null,"You will have to do roughly these steps:"),Object(r.b)("ul",null,Object(r.b)("li",{parentName:"ul"},"Define how the data in your own system maps to FHIR resources. This is the logical mapping. "),Object(r.b)("li",{parentName:"ul"},"Implement the interfaces regarding storage (see [",Object(r.b)("a",o({parentName:"li"},{href:"./architecture"}),"Architecture"),"]. Perform the actual mapping when retrieving the data for a resource from your system."),Object(r.b)("li",{parentName:"ul"},"Adjust the ConformanceBuilder, so the resulting ConformanceStatement states exactly what you support (which Resource types, which operations)."),Object(r.b)("li",{parentName:"ul"},"Use the dependency injection framework to inject your implementations for the storage interfaces (in UnityConfig.cs).")))}u&&u===Object(u)&&Object.isExtensible(u)&&Object.defineProperty(u,"__filemeta",{configurable:!0,value:{name:"MDXContent",filename:"src/pages/Deployment/Using_Spark.md"}}),u.isMDXComponent=!0}}]);
//# sourceMappingURL=component---src-pages-deployment-using-spark-md-6fdae2bc8a039d7c7084.js.map