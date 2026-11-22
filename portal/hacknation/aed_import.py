# Creates two test institutions (AED owners) with XML harvester sources pointing at their AED lists published by
# AED Radar, runs the portal's own import task and prints what the portal created.
# Run: docker compose -f docker-compose.local.yml -f hacknation/docker-compose.hacknation.yml \
#        run --rm -T --no-deps --entrypoint "" mcod-local-admin python manage.py shell < hacknation/aed_import.py
from django.contrib.auth import get_user_model

from mcod.datasets.models import Dataset
from mcod.harvester.models import DataSource
from mcod.harvester.tasks import import_data_task
from mcod.organizations.models import Organization

BASE = "https://aed.hacknation-2026.pl/api/open-data/"
PROVIDERS = [
    ("Urząd Miejski Wrocławia (demo)", "aed-a3d1c0de000240008000000000000001.xml", Organization.INSTITUTION_TYPE_LOCAL),
    ("OSP Gminy Przykładowej (demo)", "aed-a3d1c0de000240008000000000000002.xml", Organization.INSTITUTION_TYPE_OTHER),
]

admin = get_user_model().objects.get(email="admin@mcod.local")

for title, xml_file, institution_type in PROVIDERS:
    org, _ = Organization.objects.get_or_create(
        title=title,
        defaults={
            "description": f"Dysponent AED, dostawca wykazu defibrylatorów ({title}). Instytucja testowa HackNation 2026.",
            "institution_type": institution_type,
            "slug": title.lower().replace(" ", "-").replace("(", "").replace(")", ""),
            "status": "published",
        },
    )
    ds, _ = DataSource.objects.get_or_create(
        name=f"Wykaz AED – {title}",
        defaults={
            "description": "Automatyczny import wykazu defibrylatorów AED (XML 1.13) z AED Radar.",
            "source_type": "xml",
            "xml_url": BASE + xml_file,
            "status": "active",
            "frequency_in_days": 1,
            "emails": "admin@mcod.local",
            "organization": org,
            "created_by": admin,
        },
    )
    import_data_task(ds.id, force=True)
    ds.refresh_from_db()
    print(f"\n=== {title}: import {ds.last_import_status} ({ds.last_import_timestamp})")
    for dataset in Dataset.objects.filter(organization=org):
        print(f"  dataset #{dataset.id} [{dataset.status}] {dataset.title} | ext_ident={dataset.ext_ident} | license={dataset.license_code}")
        for resource in dataset.resources.all():
            print(f"    resource #{resource.id} [{resource.status}] {resource.format} {resource.type} | {resource.link}")
