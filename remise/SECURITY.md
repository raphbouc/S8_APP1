# Livrable 11 : Processus d'identification et de publication des bogues de sécurité (SECURITY.md)

**Projet CoupDeSonde - GEI-771**

Dans le cadre du projet CoupDeSonde, nous avons mis en place une politique de divulgation coordonnée des vulnérabilités pragmatique, inspirée des standards open source modernes et de notre expérience en milieu industriel.

## 1. Signalement d'une vulnérabilité (Reporting)

Si vous découvrez une faille de sécurité potentielle dans notre API ou notre infrastructure, nous vous encourageons à nous la signaler de manière responsable.

Veuillez nous contacter directement par courriel à l'adresse suivante :  
**securite@coupdesonde.local**

Dans votre courriel, merci d'inclure :

- Une description claire du problème.
- Les étapes de reproduction (une preuve de concept ou un script est l'idéal).
- L'impact potentiel que vous avez identifié.

**Critique :** Nous vous demandons expressément de ne pas divulguer publiquement la faille (sur GitHub, les réseaux sociaux ou des forums) avant que nous ayons eu l'opportunité d'analyser le problème et de déployer un correctif.

## 2. Nos engagements et délais (SLA)

En tant qu'équipe de développement, nous nous engageons à traiter les signalements avec diligence et transparence. Voici nos délais cibles (SLA) pour la gestion d'un incident de sécurité :

- **Accusé de réception** : Nous vous confirmerons la réception de votre signalement sous **48 heures**.
- **Triage et évaluation** : Sous **5 jours ouvrables**, nous évaluerons la validité du signalement et attribuerons un score de sévérité basé sur la norme CVSS (Common Vulnerability Scoring System). Vous serez tenu informé des résultats de cette analyse.
- **Déploiement d'un correctif (Patch)** : Pour toute vulnérabilité qualifiée de _critique_ (score CVSS de 9.0 ou plus) ou _élevée_ (7.0 à 8.9), nous nous engageons à développer, tester et déployer un correctif en production sous **30 jours** maximum. Les vulnérabilités moins sévères seront intégrées dans notre cycle de développement standard (backlog).

## 3. Publication et Transparence

Une fois la vulnérabilité corrigée et le correctif déployé en production, notre processus prévoit une phase de publication pour assurer la transparence envers nos utilisateurs.

Lorsqu'une mise à jour de sécurité est publiée, nous nous engageons à :

- **Publier des notes de mise à jour (Release Notes)** : Un avis de sécurité sera publié dans le dépôt de notre projet, détaillant la nature de la faille (sans nécessairement fournir la preuve de concept exacte pour éviter les abus) et les versions affectées.
- **Diffuser les mesures de mitigation** : Si des actions manuelles sont requises de la part des administrateurs système (ex. rotation des clés d'API existantes), ces étapes seront clairement documentées.
- **Remercier le chercheur** : Sauf demande d'anonymat, nous attribuerons publiquement le crédit de la découverte au chercheur en sécurité qui nous a signalé la faille de manière responsable, afin de valoriser son travail.
