IMAGE_NAME := rideit
PORT := 8080
RESOURCE_GROUP := rideit-rg
LOCATION := westeurope
ACR_NAME := clusterleansandbox
FUNC_APP_NAME := rideit

SOURCES := $(shell find rideit -name '*.cs' -o -name '*.csproj') Dockerfile

.PHONY: build run stop test benchmark clean help \
        func func-publish \
        infra infra-rg infra-deploy infra-cosmos infra-remove \
        emulator emulator-stop \
        container-infra container-build container-run container-push container-login container-clean

# --- Default: Azure Functions ---

help:
	@grep -E '^[a-zA-Z_-]+:' Makefile | cut -d: -f1 | sort

test:
	dotnet test

benchmark:
	drill --benchmark drill.yml --stats --quiet

# --- Azure Functions ---

func:
	cd rideit.Functions && func start

publish: infra-rg infra-deploy
	cd rideit.Functions && func azure functionapp publish $(FUNC_APP_NAME)

infra: infra-rg infra-cosmos infra-deploy

infra-rg:
	az group create --name $(RESOURCE_GROUP) --location $(LOCATION)

infra-deploy:
	az deployment group create \
		--resource-group $(RESOURCE_GROUP) \
		--template-file infra/functions.bicep \
		--parameters appName=$(FUNC_APP_NAME)

infra-cosmos:
	az deployment group create \
		--resource-group $(RESOURCE_GROUP) \
		--template-file infra/cosmos.bicep \
		--parameters appName=$(FUNC_APP_NAME)

infra-remove:
	az group delete --name $(RESOURCE_GROUP) --yes

# --- Local Development ---

emulator:
	podman run -d --name cosmosdb-emulator -p 8081:8081 -p 10250-10255:10250-10255 \
		mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

emulator-stop:
	podman stop cosmosdb-emulator && podman rm cosmosdb-emulator

# --- Container App ---

container-build: .image-built

.image-built: $(SOURCES)
	podman build -t $(IMAGE_NAME) .
	@touch .image-built

container-run: container-build
	podman run -d --rm --name $(IMAGE_NAME) -p $(PORT):8080 $(IMAGE_NAME)

container-stop:
	podman stop $(IMAGE_NAME)

container-login:
	az acr login --name $(ACR_NAME)

container-push: container-login
	podman tag $(IMAGE_NAME) $(ACR_NAME).azurecr.io/$(IMAGE_NAME):latest
	podman push $(ACR_NAME).azurecr.io/$(IMAGE_NAME):latest

container-infra: infra-rg container-infra-acr container-build container-push container-infra-deploy

container-infra-acr:
	az deployment group create \
		--resource-group $(RESOURCE_GROUP) \
		--template-file infra/acr.bicep \
		--parameters appName=$(ACR_NAME)

container-infra-deploy:
	$(eval ACR_PASSWORD := $(shell az acr credential show --name $(ACR_NAME) --query "passwords[0].value" -o tsv))
	$(eval ACR_USERNAME := $(shell az acr credential show --name $(ACR_NAME) --query "username" -o tsv))
	az deployment group create \
		--resource-group $(RESOURCE_GROUP) \
		--template-file infra/container.bicep \
		--parameters \
			appName=$(IMAGE_NAME) \
			containerImage=$(ACR_NAME).azurecr.io/$(IMAGE_NAME):latest \
			registryServer=$(ACR_NAME).azurecr.io \
			registryUsername=$(ACR_USERNAME) \
			registryPassword=$(ACR_PASSWORD)

container-clean:
	podman rmi $(IMAGE_NAME)
	@rm -f .image-built
