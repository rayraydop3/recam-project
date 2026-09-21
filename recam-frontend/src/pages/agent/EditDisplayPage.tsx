import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useNavigate, useParams } from 'react-router-dom'
import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'
import { Card, CardContent } from '@/components/ui/card'

import { getListingCase } from '@/services/listingCaseService'
import { addCaseContact, deleteCaseContact, getCaseContacts } from '@/services/caseContactService'

// ─── Schema ───────────────────────────────────────────────────────────────────
// Note: there is no "description" concept on the backend's ListingCase — only
// structural fields and CaseContacts — so that section from the old mock UI
// is gone. Only the Contacts section maps to a real feature.

const contactSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  companyName: z.string().min(1, 'Company is required'),
  email: z.email('Invalid email'),
  phone: z.string().min(1, 'Phone is required'),
})

type ContactFormData = z.infer<typeof contactSchema>

// ─── Component ────────────────────────────────────────────────────────────────

export default function EditDisplayPage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { id } = useParams()
  const listingId = Number(id)
  const [showContactForm, setShowContactForm] = useState(false)

  const { data: listing, isLoading: listingLoading } = useQuery({
    queryKey: ['listingCase', listingId],
    queryFn: () => getListingCase(listingId),
  })

  const { data: contacts = [], isLoading: contactsLoading } = useQuery({
    queryKey: ['caseContacts', listingId],
    queryFn: () => getCaseContacts(listingId),
  })

  const invalidateContacts = () => queryClient.invalidateQueries({ queryKey: ['caseContacts', listingId] })

  const addMutation = useMutation({
    mutationFn: (data: ContactFormData) => addCaseContact(listingId, data),
    onSuccess: () => {
      invalidateContacts()
      contactForm.reset()
      setShowContactForm(false)
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (contactId: number) => deleteCaseContact(contactId),
    onSuccess: invalidateContacts,
  })

  const contactForm = useForm<ContactFormData>({
    resolver: zodResolver(contactSchema),
    defaultValues: { firstName: '', lastName: '', companyName: '', email: '', phone: '' },
  })

  if (listingLoading || contactsLoading) {
    return <div className="p-6 text-muted-foreground">Loading...</div>
  }

  if (!listing) {
    return (
      <div className="p-6">
        <p className="text-destructive">Listing not found.</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate('/agent/listings')}>
          Back
        </Button>
      </div>
    )
  }

  return (
    <div className="p-6 max-w-2xl space-y-8">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold">Edit Display</h1>
          <p className="text-sm text-muted-foreground">{listing.address}</p>
        </div>
        <Button variant="outline" onClick={() => navigate('/agent/listings')}>
          Back
        </Button>
      </div>

      {/* Contacts */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-semibold text-muted-foreground uppercase tracking-wide">
            Contacts
          </h2>
          <Button size="sm" variant="outline" onClick={() => setShowContactForm((v) => !v)}>
            {showContactForm ? 'Cancel' : '+ Add Contact'}
          </Button>
        </div>

        {addMutation.isError && (
          <p className="text-sm text-destructive">
            {addMutation.error instanceof Error ? addMutation.error.message : 'Failed to add contact'}
          </p>
        )}

        {showContactForm && (
          <form
            onSubmit={contactForm.handleSubmit((data) => addMutation.mutate(data))}
            className="space-y-3 border rounded-lg p-4"
          >
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label htmlFor="firstName">First Name</Label>
                <Input id="firstName" {...contactForm.register('firstName')} />
                {contactForm.formState.errors.firstName && (
                  <p className="text-sm text-destructive">{contactForm.formState.errors.firstName.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="lastName">Last Name</Label>
                <Input id="lastName" {...contactForm.register('lastName')} />
                {contactForm.formState.errors.lastName && (
                  <p className="text-sm text-destructive">{contactForm.formState.errors.lastName.message}</p>
                )}
              </div>
            </div>
            <div className="space-y-1">
              <Label htmlFor="companyName">Company</Label>
              <Input id="companyName" {...contactForm.register('companyName')} />
              {contactForm.formState.errors.companyName && (
                <p className="text-sm text-destructive">{contactForm.formState.errors.companyName.message}</p>
              )}
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div className="space-y-1">
                <Label htmlFor="email">Email</Label>
                <Input id="email" type="email" {...contactForm.register('email')} />
                {contactForm.formState.errors.email && (
                  <p className="text-sm text-destructive">{contactForm.formState.errors.email.message}</p>
                )}
              </div>
              <div className="space-y-1">
                <Label htmlFor="phone">Phone</Label>
                <Input id="phone" {...contactForm.register('phone')} />
                {contactForm.formState.errors.phone && (
                  <p className="text-sm text-destructive">{contactForm.formState.errors.phone.message}</p>
                )}
              </div>
            </div>
            <Button type="submit" size="sm" disabled={addMutation.isPending}>
              {addMutation.isPending ? 'Adding...' : 'Add Contact'}
            </Button>
          </form>
        )}

        {contacts.length === 0 ? (
          <p className="text-sm text-muted-foreground">No contacts added yet.</p>
        ) : (
          <div className="space-y-2">
            {contacts.map((contact) => (
              <Card key={contact.id}>
                <CardContent className="p-4 flex items-center justify-between">
                  <div>
                    <p className="font-medium">
                      {contact.firstName} {contact.lastName}
                    </p>
                    <p className="text-sm text-muted-foreground">{contact.companyName}</p>
                    <p className="text-sm text-muted-foreground">{contact.email} &middot; {contact.phone}</p>
                  </div>
                  <Button
                    size="sm"
                    variant="destructive"
                    onClick={() => deleteMutation.mutate(contact.id)}
                    disabled={deleteMutation.isPending}
                  >
                    Remove
                  </Button>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>

      <Separator />

      <Button
        className="w-full"
        onClick={() => navigate(`/agent/listings/${id}/preview`)}
      >
        Preview Listing
      </Button>
    </div>
  )
}
