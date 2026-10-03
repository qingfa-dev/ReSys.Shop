import type { TaxonRuleForm } from '../schemas'
import type { TaxonRuleRequest } from '../types'

export class TaxonRuleFormMapper {
  static toCreate(form: TaxonRuleForm): TaxonRuleRequest {
    return {
      type: form.type,
      matchPolicy: form.matchPolicy,
      value: form.value,
    }
  }

  static toUpdate(form: TaxonRuleForm): TaxonRuleRequest {
    return TaxonRuleFormMapper.toCreate(form)
  }
}
