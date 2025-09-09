import { ChevronDown } from 'lucide-react'

interface Model {
  id: string
  object: string
  created: number
  ownedBy: string
}

interface ModelSelectorProps {
  models: Model[]
  selectedModel: string
  onModelChange: (modelId: string) => void
}

export function ModelSelector({ models, selectedModel, onModelChange }: ModelSelectorProps) {
  // const selectedModelData = models.find(m => m.id === selectedModel)

  return (
    <div className="relative">
      <select
        value={selectedModel}
        onChange={(e) => onModelChange(e.target.value)}
        className="appearance-none bg-white border border-gray-300 rounded-lg px-3 py-2 pr-8 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
      >
        {models.map((model) => (
          <option key={model.id} value={model.id}>
            {model.id}
          </option>
        ))}
      </select>
      <div className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
        <ChevronDown className="h-4 w-4 text-gray-400" />
      </div>
    </div>
  )
}
