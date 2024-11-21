export interface SiteFormData {
    siteName: string,
    description: string,
    isActive: boolean,
    landingPage: string,
    resourceMappings: Map<string, string>,
    tagIds: Array<string>
}

export class CreateOrUpdateSiteContextHolder {
    private _state: SiteFormData = {} as SiteFormData;

    public set(siteFormData: SiteFormData) {
        this._state = siteFormData;
    }

    public isModified(siteFormData: SiteFormData) : boolean {
        if (!this._state) {
            throw new Error("Originial state has not been specified.");
        }

        let areFieldsModified = 
            this._state.description != siteFormData.description ||
            this._state.isActive != siteFormData.isActive ||
            this._state.landingPage != siteFormData.landingPage ||
            this._state.siteName != siteFormData.siteName;

        if (areFieldsModified) {
            return true;
        }

        if (this._state.resourceMappings && this._state.resourceMappings.size) {
            if (!siteFormData.resourceMappings || !siteFormData.resourceMappings.size || 
                    this._state.resourceMappings.size != siteFormData.resourceMappings.size
            ) {
                return true;
            }

            let rsChanged = false;
            siteFormData.resourceMappings.forEach((val, key) => {
                let rm = this._state.resourceMappings.get(key);
                if (rm != val) {
                    rsChanged = true;
                }
            });

            if (rsChanged) {
                return true;
            }
        } else {
            if (siteFormData.resourceMappings && siteFormData.resourceMappings.size) {
                return true;
            }
        }

        if (this._state.tagIds && this._state.tagIds.length) {
            if (!siteFormData.tagIds || !siteFormData.tagIds.length || this._state.tagIds.length != siteFormData.tagIds.length) {
                return true;
            }
            
            let tagsChanged = false;
            siteFormData.tagIds.forEach((val, no) => {
                let tagId = this._state.tagIds[no];
                if (tagId != val) {
                    tagsChanged = true;
                }
            });

            if (tagsChanged) {
                return true;
            }

        } else {
            if (siteFormData.tagIds && siteFormData.tagIds.length) {
                return true;
            }
        }

        return false;
    }
}